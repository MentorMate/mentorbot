// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Core.Models.HangoutsChat
{
    /// <summary>A return result for chat event.</summary>
    public class ChatEventResult
    {
        /// <summary>Initializes a new instance of the <see cref="ChatEventResult"/> class.</summary>
        public ChatEventResult(string text)
        {
            Text = text;
        }

        /// <summary>Gets or sets the bot text.</summary>
        public string Text { get; set; }
    }
}
