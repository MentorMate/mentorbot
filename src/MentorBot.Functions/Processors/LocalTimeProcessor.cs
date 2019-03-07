// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A command that report back the current time.</summary>
    /// <seealso cref="ICommandProcessor" />
    public class LocalTimeProcessor : ICommandProcessor
    {
        private readonly Func<TimeZoneInfo> _currentTimeZoneFactory;
        private readonly Func<DateTime> _dateTimeFactory;

        /// <summary>Initializes a new instance of the <see cref="LocalTimeProcessor"/> class.</summary>
        public LocalTimeProcessor(Func<TimeZoneInfo> currentTimeZoneFactory, Func<DateTime> dateTimeFactory)
        {
            _currentTimeZoneFactory = currentTimeZoneFactory;
            _dateTimeFactory = dateTimeFactory;
        }

        /// <inheritdoc/>
        public string Subject => "Time";

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder)
        {
            var locationMatch = Regex.Match(info?.TextSentanceChunk, "in ([\\w\\s]+)$");
            if (locationMatch.Success)
            {
                var city = locationMatch.Groups[1].Value;

                // TODO: This is not a good method because it doesn't contain all cities. A better method is required.
                var timeZone = TimeZoneInfo
                    .GetSystemTimeZones()
                    .FirstOrDefault(it => it
                        .DisplayName
                        .IndexOf(city, StringComparison.InvariantCultureIgnoreCase) > -1);

                if (timeZone == null)
                {
                    return new ValueTask<ChatEventResult>(
                        new ChatEventResult($"The current time {locationMatch.Value} was not found."));
                }

                var timeInLocation = ConvertDateTimeToString(_dateTimeFactory(), timeZone);
                return new ValueTask<ChatEventResult>(
                        new ChatEventResult($"The current time {locationMatch.Value} is {timeInLocation}."));
            }

            var time = ConvertDateTimeToString(_dateTimeFactory(), _currentTimeZoneFactory());

            return new ValueTask<ChatEventResult>(
                new ChatEventResult($"The current time is {time}."));
        }

        private static string ConvertDateTimeToString(DateTime dateTime, TimeZoneInfo timeZone) =>
            TimeZoneInfo
                .ConvertTime(dateTime, timeZone)
                .ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
