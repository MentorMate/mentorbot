// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A processor that repeate back what is said.</summary>
    /// <seealso cref="ICommandProcessor" />
    public class RepeatProcessor : ICommandProcessor
    {
        private static readonly Regex RegExp = new Regex(
            "^(@mentorbot\\s+)?repeat\\s+(after me\\s+)?(delay (\\d+)\\s+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <inheritdoc/>
        public IReadOnlyList<TextDeconstructionInformation> InitalializationCommandDefinitians =>
            new[]
            {
                new TextDeconstructionInformation("Repeat", null, SentenceTypes.Command),
                new TextDeconstructionInformation("Repeat after me", null, SentenceTypes.Command)
            };

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder)
        {
            if (info == null)
            {
                return Value("I can not understand the sentance.");
            }

            var match = RegExp.Match(info.TextSentanceChunk);
            if (match.Success)
            {
                var text = info.TextSentanceChunk.Substring(match.Length);
                var delayStr = match.Groups[4]?.Value;
                if (delayStr != null &&
                    int.TryParse(delayStr, out int delayMs))
                {
                    Task.Delay(delayMs)
                        .ContinueWith(task => responder
                        .SendMessageAsync(text, originalChatEvent.Space, originalChatEvent.Message.Thread, originalChatEvent.Message.Sender));

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
