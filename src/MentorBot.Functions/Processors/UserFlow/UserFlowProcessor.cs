using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Connectors.Confluence;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.Searches;

namespace MentorBot.Functions.Processors.UserFlow
{
    /// <summary>A processor that returns answers or further questions based on chosen category or question.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class UserFlowProcessor : ICommandProcessor
    {
        private readonly IConfluenceClient _client;
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="UserFlowProcessor"/> class.</summary>
        public UserFlowProcessor(IConfluenceClient confluenceClient, IStorageService storageService)
        {
            _client = confluenceClient;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public string Name => GetType().FullName;

        /// <inheritdoc/>
        public string Subject => nameof(UserFlow);

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IPluginPropertiesAccessor accessor)
        {
            var hosts = accessor.GetPluginPropertyGroup(UserFlowProperties.HostsGroup).FirstOrDefault();
            if (hosts == null)
            {
                return new ChatEventResult("No Confluence hosts are configured.");
            }

            var user = hosts.GetValue<string>(UserFlowProperties.User);

            var state = await _storageService.GetStateAsync(user);

            if (info.TextSentenceChunk == "Frequently asked questions")
            {
                var initialQuestions = await _storageService.GetInitialQuestions();
                var initialQuestionsCard = CreateCard(initialQuestions);
                state.Active = true;
                await _storageService.AddOrUpdateStateAsync(state);

                return new ChatEventResult(initialQuestionsCard);
            }

            var index = info.TextSentenceChunk;

            var parentId = state.AnsweredQuestions.Count == 0 ? null : state.AnsweredQuestions.Last();

            var question = await _storageService.GetQuestionOrAnswerAsync(parentId, int.Parse(index));

            var nextQuestionsOrAnswer = await _storageService.GetQuestionsOrAnswerAsync(question.Id);
            state.AnsweredQuestions.Add(question.Id);

            if (nextQuestionsOrAnswer.Count == 1 && nextQuestionsOrAnswer.First().Type == QuestionAnswerType.Answer)
            {
                state.Active = false;
            }

            await _storageService.AddOrUpdateStateAsync(state);

            var card = CreateCard(nextQuestionsOrAnswer);

            return new ChatEventResult(card);
        }

        private static Card CreateCard(IReadOnlyList<QuestionAnswer> nextQuestionsOrAnswer)
        {
            return new Card
            {
                Header = new CardHeader
                {
                    Title = "Frequently asked questions"
                },
                Sections = new List<Section>
                    {
                        new Section
                        {
                            Widgets = nextQuestionsOrAnswer.Select((qa, index) => new WidgetMarkup
                            {
                                TextParagraph = new TextParagraph
                                {
                                    Text = $"{index}.{qa.Content}",
                                }
                            }).ToList(),
                        }
                    },
            };
        }
    }
}
