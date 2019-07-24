// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A processor that repeate back what is said.</summary>
    /// <seealso cref="ICommandProcessor" />
    public class RepeatProcessor : ICommandProcessor
    {
        private static readonly Regex RegExp = new Regex(
           "^(@mentorbot\\s+)?repeat( after me)?((with)? delay \\d+\\w*)?\\s*",
           RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex RegExpTime = new Regex(
            "^(\\d+)(\\w*)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <inheritdoc/>
        public string Name => nameof(RepeatProcessor);

        /// <inheritdoc/>
        public string Subject => "Repeat";

        /// <summary>Gets the number in miliseconds from a time string.</summary>
        public static int GetTime(string value)
        {
            var match = RegExpTime.Match(value);
            var val = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            switch (match.Groups[2].Value)
            {
                case "h":
                case "hour":
                    return val * 3600000;
                case "min":
                case "m":
                    return val * 60000;
                case "s":
                case "sec":
                    return val * 1000;
                default:
                    return val;
            }
        }

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder, IReadOnlyDictionary<string, string> settings)
        {
            var text = RegExp.Replace(info.TextSentanceChunk, string.Empty);
            var delayStr = info.Entities.GetValueOrDefault("Time")?.FirstOrDefault();
            if (!string.IsNullOrEmpty(text))
            {
                if (!string.IsNullOrEmpty(delayStr))
                {
                    var delay = GetTime(delayStr);
                    Task.Delay(delay)
                        .ContinueWith(task => responder.SendMessageAsync(text, new GoogleChatAddress(originalChatEvent)));

                    return Value(null);
                }

                return Value(text);
            }

            return Value("Repeat command can not recognise some segments.");
        }

        private static ValueTask<ChatEventResult> Value(string text) =>
            new ValueTask<ChatEventResult>(
                result: string.IsNullOrEmpty(text) ? null : new ChatEventResult(text));
    }
}
