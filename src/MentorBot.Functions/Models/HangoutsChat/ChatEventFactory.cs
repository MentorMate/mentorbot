// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using Google.Apis.HangoutsChat.v1.Data;

namespace MentorBot.Functions.Models.HangoutsChat
{
    /// <summary>A object creation factory that create all chat objects.</summary>
    public static class ChatEventFactory
    {
        /// <summary>Creates a result card from key-value widget.</summary>
        /// <param name="keyValue">The key value widget.</param>
        public static Card CreateCard(KeyValue keyValue) =>
            new Card
            {
                Sections = new[]
                {
                    new Section
                    {
                        Widgets = new[]
                        {
                            new WidgetMarkup
                            {
                                KeyValue = keyValue
                            }
                        }
                    }
                }
            };

        /// <summary>Creates a result card from text paragraph widget.</summary>
        public static Card CreateCard(TextParagraph textParagraph) =>
            new Card
            {
                Sections = new[]
                {
                    new Section
                    {
                        Widgets = new[]
                        {
                            new WidgetMarkup
                            {
                                TextParagraph = textParagraph
                            }
                        }
                    }
                }
            };

        /// <summary>Creates a result card text button.</summary>
        /// <param name="label">The button label.</param>
        /// <param name="url">The button URL.</param>
        public static Button CreateTextButton(string label, string url) =>
            new Button
            {
                TextButton = new TextButton
                {
                    Text = label,
                    OnClick = new OnClick
                    {
                        OpenLink = new OpenLink
                        {
                            Url = url
                        }
                    }
                }
            };
    }
}
