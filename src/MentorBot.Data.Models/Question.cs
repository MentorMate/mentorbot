// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using MentorBot.Data.Common.Models;

namespace MentorBot.Data.Models
{
    /// <summary>
    /// The question database entity.
    /// </summary>
    /// <seealso cref="MentorBot.Data.Common.Models.BaseModel{long}" />
    public class Question : BaseModel<long>
    {
        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string QuestionText { get; set; }

        /// <summary>
        /// Gets or sets the Answer identifier.
        /// </summary>
        public long AnswerId { get; set; }

        /// <summary>
        /// Gets or sets the answer>.
        /// </summary>
        public Answer Answer { get; set; }
    }
}
