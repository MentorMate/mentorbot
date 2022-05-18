using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
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

            var input = info.TextSentenceChunk;

            if (!state.Active)
            {
                var questionsWithoutParents = QuestionsWithoutParents(questions);
                if (questions == null || questions.Count == 0)
                {
                    return ExitCard("There aren't any questions in the database at the moment.");
                }

                var mentorMaterTypesCard = CreateCard(questionsWithoutParents, "Select your Mentor Mater Type.");
                await SetInitialState(state, questions, input);

                return new ChatEventResult(mentorMaterTypesCard);
            }

            var parentId = state.CurrentQuestionId;

            var relativeQuestions = GetRelativeQuestions(state, questions, parentId);

            if (InvalidAnswer(input, relativeQuestions.Count))
            {
                var repeatCard = CreateCard(relativeQuestions, "Select a category/question.");
                return new ChatEventResult(repeatCard);
            }

            if (UserWantsToExit(input, relativeQuestions.Count))
            {
                await ResetState(state);
                return ExitCard("Exiting");
            }

            var question = relativeQuestions[int.Parse(input) - 1];

            if (question.AcquireTraits != null)
            {
                state.Traits.AddRange(question.AcquireTraits);
            }

            var questionId = question.Id;

            if (!string.IsNullOrWhiteSpace(state.EntryQuestionId))
            {
                questionId = state.EntryQuestionId;
                state.EntryQuestionId = string.Empty;
            }

            var nextQuestionsOrAnswer = GetRelativeQuestionsWithParentsAndTraits(state, questions, questionId);

            state.CurrentQuestionId = questionId;

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

        private static ChatEventResult ExitCard(string message)
        {
            var exitCard = CreateCard(null, message);
            return new ChatEventResult(exitCard);
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
            else if (nextQuestionsOrAnswer.Count == 1 && nextQuestionsOrAnswer[0].IsAnswer)
            {
                return new List<WidgetMarkup> { WidgetWithAnswer(nextQuestionsOrAnswer[0]) };
            }
            else
            {
                return WidgetWithQuestions(nextQuestionsOrAnswer);
            }
        }

        private static List<WidgetMarkup> WidgetWithQuestions(IReadOnlyList<QuestionAnswer> nextQuestionsOrAnswer)
        {
            return nextQuestionsOrAnswer.Select((q, index) => new WidgetMarkup
            {
                TextParagraph = new TextParagraph
                {
                    Text = $"{index + 1}.{q.Title}",
                },
            }).ToList();
        }

        private static WidgetMarkup WidgetWithAnswer(QuestionAnswer answer)
        {
            return new WidgetMarkup
            {
                TextParagraph = new TextParagraph
                {
                    Text = TextWithClickableLinks(answer.Content),
                }
            };
        }

        private static string TextWithClickableLinks(string answer)
        {
            answer = Regex.Replace(
                answer,
                @"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
                "<a target='_blank' href='$1'>$1</a>");

            return answer;
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
            if (string.IsNullOrEmpty(parentId) || parentId == "null")
            {
                return QuestionsWithoutParents(questions).ToList();
            }
            else
            {
                return GetRelativeQuestionsWithParentsAndTraits(state, questions, parentId).ToList();
            }
        }

        private async Task SetInitialState(State state, IReadOnlyList<QuestionAnswer> questions, string input)
        {
            if (questions.Any(q => q.Title == input))
            {
                state.EntryQuestionId = questions.First(q => q.Title == input).Id;
            }

            state.Active = true;
            await _storageService.AddOrUpdateStateAsync(state);
        }

        private async Task ResetState(State state)
        {
            state.Active = false;
            state.CurrentQuestionId = string.Empty;
            state.EntryQuestionId = string.Empty;
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
