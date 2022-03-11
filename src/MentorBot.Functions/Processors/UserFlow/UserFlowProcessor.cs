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
        public string Subject => "Userflow";

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
                return new ChatEventResult("No UserFlow hosts are configured.");
            }

            var user = hosts.GetValue<string>(UserFlowProperties.User);

            var state = await _storageService.GetStateAsync(user);

            if (state == null)
            {
                var newState = new State
                {
                    UserEmail = user,
                };

                await _storageService.AddOrUpdateStateAsync(newState);
                state = await _storageService.GetStateAsync(user);
            }

            if (!state.Active)
            {
                var mentorMaterTypes = await _storageService.GetMentorMaterTypes();
                var mentorMaterTypesCard = CreateCard(mentorMaterTypes, "Select your Mentor Mater Type.");
                state.Active = true;
                await _storageService.AddOrUpdateStateAsync(state);

                return new ChatEventResult(mentorMaterTypesCard);
            }

            var index = info.TextSentenceChunk;

            if (state.AnsweredQuestions == null || state.AnsweredQuestions.Count == 0)
            {
                state.AnsweredQuestions = new List<string>();
                state.AnsweredQuestions.Add(index);
                await _storageService.AddOrUpdateStateAsync(state);
                var initialQuestions = await _storageService.GetInitialQuestionsAsync();
                var relativeInitialQuestions = initialQuestions.Where(q => q.MentorMaterType[int.Parse(index) - 1]).ToList();
                relativeInitialQuestions = relativeInitialQuestions.Where(q => q.ParentId == null).ToList();
                var initialQuestionsCard = CreateCard(relativeInitialQuestions, "Select a category.");

                return new ChatEventResult(initialQuestionsCard);
            }

            var parentId = state.AnsweredQuestions.Count == 1 ? null : state.AnsweredQuestions.Last();

            var questions = await _storageService.GetQuestionsOrAnswerAsync(parentId);

            var relativeQuestions = questions.Where(q => q.MentorMaterType[int.Parse(state.AnsweredQuestions.First()) - 1]).ToList();

            if (parentId == null)
            {
                relativeQuestions = relativeQuestions.Where(q => q.ParentId == null).ToList();
            }

            var question = relativeQuestions[int.Parse(index) - 1];

            var nextQuestionsOrAnswerQuery = await _storageService.GetQuestionsOrAnswerAsync(question.Id);
            state.AnsweredQuestions.Add(question.Id);

            var nextQuestionsOrAnswer = nextQuestionsOrAnswerQuery.Where(q => q.MentorMaterType[int.Parse(state.AnsweredQuestions.First()) - 1]).ToList();

            if (nextQuestionsOrAnswer.Any(x => x.Type == "2" && x.MentorMaterType[int.Parse(state.AnsweredQuestions.First()) - 1]))
            {
                state.Active = false;
                var answer = nextQuestionsOrAnswer
                    .First(x => x.Type == "2"
                    && x.MentorMaterType[int.Parse(state.AnsweredQuestions.First()) - 1]);

                state.AnsweredQuestions.RemoveAll(s => s != null);

                await _storageService.AddOrUpdateStateAsync(state);

                var result = new List<QuestionAnswer>();

                result.Add(answer);

                var answerCard = CreateCard(result, answer.Title);

                return new ChatEventResult(answerCard);
            }

            await _storageService.AddOrUpdateStateAsync(state);

            var card = CreateCard(nextQuestionsOrAnswer, "Select a category/question.");

            return new ChatEventResult(card);
        }

        private static Card CreateCard(IReadOnlyList<QuestionAnswer> nextQuestionsOrAnswer, string title)
        {
            return new Card
            {
                Header = new CardHeader
                {
                    Title = title,
                },
                Sections = new List<Section>
                    {
                        new Section
                        {
                            Widgets = nextQuestionsOrAnswer.Select((qa, index) => new WidgetMarkup
                            {
                                TextParagraph = new TextParagraph
                                {
                                    Text = $"{(nextQuestionsOrAnswer.Any(q => q.Type == "2") ? string.Empty : index + 1 + ".")}" +
                                    $"{(string.IsNullOrWhiteSpace(qa.Content) ? qa.Title : qa.Content)}",
                                }
                            }).ToList(),
                        }
                    },
            };
        }
    }
}
