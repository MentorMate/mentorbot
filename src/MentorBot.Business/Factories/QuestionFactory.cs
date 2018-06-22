// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

using MentorBot.Data.Models;

namespace MentorBot.Business.Factories
{
    /// <summary>
    /// Factory class that contains static methods for creating data <see cref="Question"/> instances.
    /// </summary>
    public static class QuestionFactory
    {
        /// <summary>
        /// Gets the question from text.
        /// </summary>
        public static Func<string, Question> GetQuestion
            => question
                => new Question
                {
                    QuestionText = question,
                    Answer = new Answer()
                };
    }
}
