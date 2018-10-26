// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

namespace MentorBot.Functions.Models.HangoutsChat
{
    /// <summary>'Hangouts Chat' event payload model.</summary>
    public class ChatEvent
    {
        /// <summary>Gets or sets the message type.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets the google secret token.</summary>
        public string Token { get; set; }

        /// <summary>Gets or sets the event time.</summary>
        public DateTime EventTime { get; set; }

        /// <summary>Gets or sets the space information.</summary>
        public ChatEventSpace Space { get; set; }

        /// <summary>Gets or sets the event message.</summary>
        public ChatEventMessage Message { get; set; }
    }
}
