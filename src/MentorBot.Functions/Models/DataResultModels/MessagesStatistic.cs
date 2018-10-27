// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Models.DataResultModels
{
    /// <summary>Message statistic result model.</summary>
    public sealed class MessagesStatistic
    {
        /// <summary>Gets or sets the probability percent of correct answer.</summary>
        public byte ProbabilityPercentage { get; set; }

        /// <summary>Gets or sets the count number.</summary>
        public int Count { get; set; }
    }
}
