// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using MentorBot.Functions.Models.HangoutsChat;

namespace MentorBot.Functions.Models.Business
{
    /// <summary>A message address of google chat.</summary>
    public sealed class GoogleChatAddress
    {
        /// <summary>Initializes a new instance of the <see cref="GoogleChatAddress"/> class.</summary>
        public GoogleChatAddress(ChatEvent chatEvent)
        {
            Space = chatEvent.Space;
            Sender = chatEvent.Message.Sender;
            ThreadName = chatEvent.Message.Thread?.Name;
        }

        /// <summary>Initializes a new instance of the <see cref="GoogleChatAddress"/> class.</summary>
        public GoogleChatAddress(string spaceName, string spaceDisplayName, string spaceType, string senderName, string senderDisplayName)
        {
            Space = new ChatEventSpace
            {
                Name = spaceName,
                DisplayName = spaceDisplayName,
                Type = spaceType
            };

            Sender = new ChatEventMessageSender
            {
                Name = senderName,
                DisplayName = senderDisplayName
            };
        }

        /// <summary>Initializes a new instance of the <see cref="GoogleChatAddress"/> class.</summary>
        public GoogleChatAddress(ChatEventSpace space, ChatEventMessageSender sender)
        {
            Space = space;
            Sender = sender;
        }

        /// <summary>Gets or sets the space information.</summary>
        public ChatEventSpace Space { get; set; }

        /// <summary>Gets or sets the name of the thread.</summary>
        public string ThreadName { get; set; }

        /// <summary>Gets or sets the sender.</summary>
        public ChatEventMessageSender Sender { get; set; }
    }
}
