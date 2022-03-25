using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors.UserFlow
{
    /// <summary>A processor that returns answers or further questions based on chosen category or question.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class UserFlowProcessor : ICommandProcessor
    {
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="UserFlowProcessor"/> class.</summary>
        public UserFlowProcessor(IStorageService storageService)
        {
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

            var questions = await _storageService.GetAllQuestionsAsync();

            var relativeQuestions = questions.ToList();

            if (!state.Active)
            {
                relativeQuestions = relativeQuestions.Where(q => q.Parents == null || q.Parents.Count == 0).OrderBy(q => q.Index).ToList();
                var mentorMaterTypesCard = CreateCard(relativeQuestions, "Select your Mentor Mater Type.");
                state.Active = true;
                await _storageService.AddOrUpdateStateAsync(state);

                return new ChatEventResult(mentorMaterTypesCard);
            }

            var index = info.TextSentenceChunk;

            var parentId = state.CurrentQuestionId;

            if (string.IsNullOrEmpty(parentId) || parentId == "null")
            {
                relativeQuestions = relativeQuestions.Where(q => q.Parents == null || q.Parents.Count == 0).ToList();
            }
            else if (state.Traits.Count == 0)
            {
                relativeQuestions = relativeQuestions.Where(q => q.Parents.ContainsKey(parentId)).ToList();
            }
            else
            {
                relativeQuestions = questions
                .Where(q => q.RequiredTraits.Any(t => state.Traits.FirstOrDefault(st => st == t) != null)
                && q.Parents.ContainsKey(parentId)).ToList();
            }

            var question = relativeQuestions[int.Parse(index) - 1];

            state.Traits.AddRange(question.AcquireTraits);

            var nextQuestionsOrAnswer = questions
                .Where(q => q.Parents.ContainsKey(question.Id)
            && q.RequiredTraits.Any(t => state.Traits.FirstOrDefault(st => st == t) != null))
                .ToList();

            state.CurrentQuestionId = question.Id;

            if (nextQuestionsOrAnswer.Any(x => x.IsAnswer))
            {
                state.Active = false;
                var answer = nextQuestionsOrAnswer
                    .First(x => x.IsAnswer);

                state.CurrentQuestionId = string.Empty;
                state.Traits.RemoveAll(t => t != null);

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
                                    Text = $"{(nextQuestionsOrAnswer.Any(q => q.IsAnswer) ? string.Empty : index + 1 + ".")}" +
                                    $"{(string.IsNullOrWhiteSpace(qa.Content) ? qa.Title : qa.Content)}",
                                }
                            }).ToList(),
                        }
                    },
            };
        }
    }
}
