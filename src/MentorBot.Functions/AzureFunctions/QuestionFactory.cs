using System.Collections.Generic;
using System.Linq;

using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.ViewModels;

namespace MentorBot.Functions.AzureFunctions
{
    /// <summary> Factory for converting questionAnswer to questionAnswerViewModel. </summary>
    public class QuestionFactory
    {
        private QuestionFactory(IEnumerable<QuestionAnswer> questionAnswers)
        {
            QuestionAnswers = questionAnswers
                .Select(q => new QuestionAnswerViewModel
                {
                    Id = q.Id,
                    Content = q.Content,
                    AcquireTraits = q.AcquireTraits,
                    RequiredTraits = q.RequiredTraits,
                    Parents = q.Parents,
                    Title = q.Title,
                    IsAnswer = q.IsAnswer,
                    SubQuestions = questionAnswers
                    .Where(d => d.Parents != null && d.Parents.Keys.Contains(q.Id))
                    .Select(d => new QuestionAnswerViewModel
                    {
                        Id = d.Id,
                        Content = d.Content,
                        AcquireTraits = d.AcquireTraits,
                        RequiredTraits = d.RequiredTraits,
                        Parents = d.Parents,
                        Title = d.Title,
                        IsAnswer = d.IsAnswer,
                    })
                    .ToArray(),
                });
        }

        /// <summary> Gets questionAnswer. </summary>
        public IEnumerable<QuestionAnswerViewModel> QuestionAnswers { get; private set; }

        /// <summary> Converts questionAnswer to questionAnswerViewModel questionAnswer. </summary>
        public static QuestionFactory Create(IEnumerable<QuestionAnswer> questionAnswers)
        {
            return new QuestionFactory(questionAnswers);
        }
    }
}
