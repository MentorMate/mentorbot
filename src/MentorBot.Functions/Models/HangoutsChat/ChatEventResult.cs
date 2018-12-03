// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using Google.Apis.HangoutsChat.v1.Data;

namespace MentorBot.Functions.Models.HangoutsChat
{
    /// <summary>A return result for chat event. We can use <see cref="Google.Apis.HangoutsChat.v1.Data.Message"/>, but this is simpler object.</summary>
    /// <seealso cref="Message"/>
    public class ChatEventResult
    {
        /// <summary>Initializes a new instance of the <see cref="ChatEventResult"/> class.</summary>
        public ChatEventResult(string text)
        {
            Text = text;
        }

        /// <summary>Initializes a new instance of the <see cref="ChatEventResult"/> class.</summary>
        public ChatEventResult(params Card[] cards)
        {
            Cards = cards;
        }

        /// <summary>Gets or sets the bot text.</summary>
        public string Text { get; protected set; }

        /// <summary>Gets or sets the cards.</summary>
        public IReadOnlyList<Card> Cards { get; protected set; }
    }
}
