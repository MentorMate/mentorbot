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
        private readonly Func<TimeZoneInfo> _currentTimeZoneFactory;

        /// <summary>Initializes a new instance of the <see cref="CalendarProcessor"/> class.</summary>
        public CalendarProcessor(IGoogleCalendarConnector googleCalendarConnector, Func<TimeZoneInfo> currentTimeZoneFactory)
        {
            _googleCalendarConnector = googleCalendarConnector;
            _currentTimeZoneFactory = currentTimeZoneFactory;
        }

        /// <inheritdoc/>
        public string Name => nameof(CalendarProcessor);

        /// <inheritdoc/>
        public string Subject => "Meetings";

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder, IReadOnlyDictionary<string, string> settings)
        {
            Contract.Ensures(info != null, "Text deconstruction information is required!");

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
            var startDate = item.Start.DateTime.HasValue ?
                TimeZoneInfo.ConvertTime(item.Start.DateTime.Value, _currentTimeZoneFactory()).ToString("HH:mm", CultureInfo.InvariantCulture) :
                null;
            var keyValue = new KeyValue
            {
                Content = item.Summary,
                ContentMultiline = false,
                BottomLabel = startDate,
                Icon = "INVITE",
                Button = link == null ? null : ChatEventFactory.CreateTextButton("JOIN", link.Uri)
            };

            var card = ChatEventFactory.CreateCard(keyValue);
            return new ChatEventResult(card);
        }
    }
}
