// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.ComponentModel.DataAnnotations;

using MentorBot.Data.Common.Models;

namespace MentorBot.Data.Models
{
    /// <summary>
    /// The answer database entity.
    /// </summary>
    /// <seealso cref="MentorBot.Data.Common.Models.BaseModel{long}" />
    public class Answer : BaseModel<long>
    {
        /// <summary>
        /// Gets or sets the <see cref="Question"/> text.
        /// </summary>
        public string AnswerText { get; set; }
    }
}
