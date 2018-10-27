// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Models.HangoutsChat
{
    /// <summary>The model contains information about 'Hangouts Chat' event -> space.</summary>
    /// <seealso cref="ChatEvent"/>
    public class ChatEventSpace
    {
        /// <summary>Gets or sets the space name.</summary>
        /// <example>spaces/AAAAAAAAAAA</example>
        public string Name { get; set; }

        /// <summary>Gets or sets the space display name.</summary>
        /// <example>Chuck Norris Discussion Room</example>
        public string DisplayName { get; set; }

        /// <summary>Gets or sets the space type.</summary>
        /// <example>ROOM, DM</example>
        public string Type { get; set; }
    }
}
