// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

namespace MentorBot.Functions.Models.HangoutsChat
{
    /// <summary>The model contains information about the user that sended 'Hangouts Chat' message.</summary>
    public class ChatEventMessageSender
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the user display name.</summary>
        public string DisplayName { get; set; }

        /// <summary>Gets or sets the user email.</summary>
        public string Email { get; set; }

        /// <summary>Gets or sets the avatar url.</summary>
        public Uri AvatarUrl { get; set; }
    }
}
