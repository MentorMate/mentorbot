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
            var user = originalChatEvent.Message.Sender.Email;

            var state = await _storageService.GetStateAsync(user);

            if (state == null)
            {
                state = await CreateStateAsync(user);
            }

            var questions = await _storageService.GetAllQuestionsAsync();

            if (!state.Active)
            {
                questions = QuestionsWithoutParents(questions);
                var mentorMaterTypesCard = CreateCard(questions, "Select your Mentor Mater Type.");

                state.Active = true;
                await _storageService.AddOrUpdateStateAsync(state);

                return new ChatEventResult(mentorMaterTypesCard);
            }

            var index = info.TextSentenceChunk;

            var parentId = state.CurrentQuestionId;

            var relativeQuestions = GetRelativeQuestions(state, questions, parentId);

            if (InvalidAnswer(index, relativeQuestions.Count))
            {
                var repeatCard = CreateCard(relativeQuestions, "Select a category/question.");
                return new ChatEventResult(repeatCard);
            }

            if (UserWantsToExit(index, relativeQuestions.Count))
            {
                await ResetState(state);
                var exitCard = CreateCard(null, "Exiting");
                return new ChatEventResult(exitCard);
            }

            var question = relativeQuestions[int.Parse(index) - 1];

            if (question.AcquireTraits != null)
            {
                state.Traits.AddRange(question.AcquireTraits);
            }

            var nextQuestionsOrAnswer = GetRelativeQuestionsWithParentsAndTraits(state, questions, question.Id);

            state.CurrentQuestionId = question.Id;

            if (nextQuestionsOrAnswer.Any(x => x.IsAnswer))
            {
                var answer = nextQuestionsOrAnswer
                    .First(x => x.IsAnswer);

                await ResetState(state);

                var result = new List<QuestionAnswer>();
                result.Add(answer);

                var answerCard = CreateCard(result, answer.Title);

                return new ChatEventResult(answerCard);
            }

            await _storageService.AddOrUpdateStateAsync(state);

            var cardTitle = "Select a category/question.";

            if (nextQuestionsOrAnswer.Count == 0)
            {
                await ResetState(state);
                cardTitle = "No answers found";
            }

            var card = CreateCard(nextQuestionsOrAnswer, cardTitle);

            return new ChatEventResult(card);
        }

        private static IReadOnlyList<QuestionAnswer> QuestionsWithoutParents(IReadOnlyList<QuestionAnswer> questions) =>
            questions = questions.Where(q => q.Parents == null || q.Parents.Count == 0).ToList();

        private static Card CreateCard(IReadOnlyList<QuestionAnswer> nextQuestionsOrAnswer, string title)
        {
            var card = new Card
            {
                Header = new CardHeader
                {
                    Title = title,
                },
                Sections = new List<Section>
                {
                    new Section
                    {
                        Widgets = GetWidgets(nextQuestionsOrAnswer)
                    }
                },
            };

            if (nextQuestionsOrAnswer != null && !nextQuestionsOrAnswer.Any(q => q.IsAnswer) && nextQuestionsOrAnswer.Count != 0)
            {
                card.Sections[0].Widgets.Add(new WidgetMarkup
                {
                    TextParagraph = new TextParagraph
                    {
                        Text = $"{nextQuestionsOrAnswer.Count + 1}.Exit",
                    }
                });
            }

            return card;
        }

        private static List<WidgetMarkup> GetWidgets(IReadOnlyList<QuestionAnswer> nextQuestionsOrAnswer)
        {
            if (nextQuestionsOrAnswer == null)
            {
                return WidgetWithMessage("Exiting chat flow");
            }
            else if (nextQuestionsOrAnswer.Count == 0)
            {
                return WidgetWithMessage("There aren't any answers added to this question yet.Exiting current mode");
            }
            else
            {
                return WidgetWithQuestionsOrAnswer(nextQuestionsOrAnswer);
            }
        }

        private static List<WidgetMarkup> WidgetWithQuestionsOrAnswer(IReadOnlyList<QuestionAnswer> nextQuestionsOrAnswer)
        {
            return nextQuestionsOrAnswer.Select((qa, index) => new WidgetMarkup
            {
                TextParagraph = new TextParagraph
                {
                    Text = $"{(nextQuestionsOrAnswer.Any(q => q.IsAnswer) ? string.Empty : index + 1 + ".")}" +
                                                        $"{(string.IsNullOrWhiteSpace(qa.Content) ? qa.Title : qa.Content)}",
                }
            }).ToList();
        }

        private static List<WidgetMarkup> WidgetWithMessage(string text)
        {
            return new List<WidgetMarkup>
            {
                new WidgetMarkup
                {
                    TextParagraph = new TextParagraph
                    {
                        Text = text,
                    }
                }
            };
        }

        private static List<QuestionAnswer> GetRelativeQuestionsWithParentsAndTraits(
            State state,
            IReadOnlyList<QuestionAnswer> questions,
            string questionId)
        {
            return questions
                            .Where(q => q.Parents.ContainsKey(questionId)
                        && q.RequiredTraits.Any(t => state.Traits.FirstOrDefault(st => st == t) != null))
                            .ToList();
        }

        private static IReadOnlyList<QuestionAnswer> GetRelativeQuestions(
            State state,
            IReadOnlyList<QuestionAnswer> questions,
            string parentId)
        {
            var relativeQuestions = new List<QuestionAnswer>();
            if (string.IsNullOrEmpty(parentId) || parentId == "null")
            {
                relativeQuestions = QuestionsWithoutParents(questions).ToList();
            }
            else if (state.Traits.Count == 0)
            {
                relativeQuestions = relativeQuestions.Where(q => q.Parents.ContainsKey(parentId)).ToList();
            }
            else
            {
                relativeQuestions = GetRelativeQuestionsWithParentsAndTraits(state, questions, parentId).ToList();
            }

            return relativeQuestions;
        }

        private async Task ResetState(State state)
        {
            state.Active = false;
            state.CurrentQuestionId = string.Empty;
            state.Traits.RemoveAll(t => t != null);

            await _storageService.AddOrUpdateStateAsync(state);
        }

        private async Task<State> CreateStateAsync(string user)
        {
            var newState = new State
            {
                UserEmail = user,
            };

            await _storageService.AddOrUpdateStateAsync(newState);
            var state = await _storageService.GetStateAsync(user);
            return state;
        }

        private bool UserWantsToExit(string input, int questionsCount)
        {
            return int.Parse(input) - 1 == questionsCount;
        }

        private bool InvalidAnswer(string input, int questionsCount)
        {
            var index = 0;

            if (!int.TryParse(input, out index))
            {
                return true;
            }

            return index > questionsCount + 1;
        }
    }
}
