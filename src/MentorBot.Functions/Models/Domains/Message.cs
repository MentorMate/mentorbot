// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using MentorBot.Functions.Models.HangoutsChat;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The congnitive service result.</summary>
    public sealed class Message
    {
        /// <summary>Gets or sets the user message.</summary>
        public string Input { get; set; }

        /// <summary>Gets or sets the event result.</summary>
        public ChatEventResult Output { get; set; }

        /// <summary>Gets or sets the confidence procent from 0 to 100. 0 is not able to tell the comand and 100 is exact match.</summary>
        public byte ProbabilityPercentage { get; set; }
    }
}
