// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
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
        /// <inheritdoc/>
        public IReadOnlyList<TextDeconstructionInformation> InitalializationCommandDefinitians =>
            new[]
            {
                new TextDeconstructionInformation("What is the current time", "time", SentenceTypes.Question, "current"),
                new TextDeconstructionInformation("What is the time", "time", SentenceTypes.Question),
                new TextDeconstructionInformation("What is the local time", "time", SentenceTypes.Question, "local"),
                new TextDeconstructionInformation("What time it is", "time", SentenceTypes.Question, "be"),
                new TextDeconstructionInformation("What time is it", "time", SentenceTypes.Question, "be")
            };

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder)
        {
            var message = originalChatEvent?.Message ?? throw new ArgumentNullException(nameof(originalChatEvent));
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

                var timeInLocation = TimeZoneInfo.ConvertTime(message.CreateTime, timeZone);
                return new ValueTask<ChatEventResult>(
                        new ChatEventResult($"The current time {locationMatch.Value} is {timeInLocation.ToLongTimeString()}."));
            }

            var time = originalChatEvent?.EventTime.ToLongTimeString() ?? string.Empty;

            return new ValueTask<ChatEventResult>(
                new ChatEventResult($"The current time is {time} UTC."));
        }
    }
}
