// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

namespace MentorBot.Functions.Models.HangoutsChat
{
    /// <summary>The model contains information about 'Hangouts Chat' event -> message.</summary>
    /// <seealso cref="ChatEvent"/>
    public class ChatEventMessage
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the create time.</summary>
        public DateTime CreateTime { get; set; }

        /// <summary>Gets or sets the message text.</summary>
        public string Text { get; set; }

        /// <summary>Gets or sets the thread.</summary>
        public ChatEventMessageThread Thread { get; set; }

        /// <summary>Gets or sets the sender.</summary>
        public ChatEventMessageSender Sender { get; set; }
    }
}
