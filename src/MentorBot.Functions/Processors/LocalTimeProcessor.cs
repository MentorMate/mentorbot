using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Connectors.BingMaps;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

using TimeZoneConverter;

namespace MentorBot.Functions.Processors
{
    /// <summary>A command that report back the current time.</summary>
    /// <seealso cref="ICommandProcessor" />
    public class LocalTimeProcessor : ICommandProcessor
    {
        private readonly Func<TimeZoneInfo> _currentTimeZoneFactory;
        private readonly Func<DateTime> _dateTimeFactory;
        private readonly IBingMapsClient _client;

        /// <summary>Initializes a new instance of the <see cref="LocalTimeProcessor"/> class.</summary>
        public LocalTimeProcessor(
            IBingMapsClient client,
            Func<TimeZoneInfo> currentTimeZoneFactory,
            Func<DateTime> dateTimeFactory)
        {
            _client = client;
            _currentTimeZoneFactory = currentTimeZoneFactory;
            _dateTimeFactory = dateTimeFactory;
        }

        /// <inheritdoc/>
        public string Name => GetType().FullName;

        /// <inheritdoc/>
        public string Subject => "Time";

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IPluginPropertiesAccessor accessor)
        {
            var locationMatch = Regex.Match(info?.TextSentenceChunk, "in ([\\w\\s]+)$");
            if (locationMatch.Success)
            {
                var city = locationMatch.Groups[1].Value;
                var data = await _client.QueryAsync(city);
                var timeZone = data == null ? null : TZConvert.GetTimeZoneInfo(data.GenericName);
                if (timeZone == null)
                {
                    return new ChatEventResult($"The current time {locationMatch.Value} was not found.");
                }

                var timeInLocation = ConvertDateTimeToString(_dateTimeFactory(), timeZone);
                return new ChatEventResult($"The current time {locationMatch.Value} is {timeInLocation}.");
            }

            var time = ConvertDateTimeToString(_dateTimeFactory(), _currentTimeZoneFactory());

            return new ChatEventResult($"The current time is {time}.");
        }

        private static string ConvertDateTimeToString(DateTime dateTime, TimeZoneInfo timeZone) =>
            TimeZoneInfo
                .ConvertTime(dateTime, timeZone)
                .ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
