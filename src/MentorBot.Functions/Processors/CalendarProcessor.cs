// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A command that provide an information and setup calendar event, meetings and remainders.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class CalendarProcessor : ICommandProcessor
    {
        private readonly IGoogleCalendarConnector _googleCalendarConnector;

        /// <summary>Initializes a new instance of the <see cref="CalendarProcessor"/> class.</summary>
        public CalendarProcessor(IGoogleCalendarConnector googleCalendarConnector)
        {
            _googleCalendarConnector = googleCalendarConnector;
        }

        /// <inheritdoc/>
        public IReadOnlyList<TextDeconstructionInformation> InitalializationCommandDefinitians =>
            new[]
            {
                new TextDeconstructionInformation("What is my next meeting", "meeting", SentenceTypes.Question),
                new TextDeconstructionInformation("What is my next event", "event", SentenceTypes.Question),
                new TextDeconstructionInformation("Show me my next meeting", "meeting", SentenceTypes.Question),
                new TextDeconstructionInformation("Show me my next event", "event", SentenceTypes.Question),
                new TextDeconstructionInformation("Get my next meeting", "meeting", SentenceTypes.Question),
                new TextDeconstructionInformation("Get my next event", "event", SentenceTypes.Question),
                new TextDeconstructionInformation("What is next on my calendar", "calendar", SentenceTypes.Question)
            };

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder)
        {
            if (info == null)
            {
                return new ChatEventResult("I do not understand the sentance.");
            }

            var sender = originalChatEvent?.Message.Sender ??
                throw new ArgumentNullException(nameof(originalChatEvent));

            var item = await _googleCalendarConnector
                .GetNextMeetingAsync(sender.Email)
                .ConfigureAwait(false);

            if (item?.Summary == null)
            {
                return new ChatEventResult("Can not find your next event! You may have no events or the service user account do not see your calendar.");
            }

            var link = item.ConferenceData?.EntryPoints.FirstOrDefault();
            var keyValue = new KeyValue
            {
                Content = item.Summary,
                ContentMultiline = false,
                BottomLabel = item.Start.DateTime?.TimeOfDay.ToString(),
                Icon = "INVITE",
                Button = link == null ? null : ChatEventFactory.CreateTextButton("JOIN", link.Uri)
            };

            var card = ChatEventFactory.CreateCard(keyValue);
            return new ChatEventResult(card);
        }
    }
}
