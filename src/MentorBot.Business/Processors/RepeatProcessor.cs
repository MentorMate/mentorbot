// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Core.Abstract.Processor;
using MentorBot.Core.Models.HangoutsChat;
using MentorBot.Core.Models.TextAnalytics;

namespace MentorBot.Business.Processors
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
        public async ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder)
        {
            if (info == null)
            {
                return new ChatEventResult("I can not understand the sentance.");
            }

            var match = RegExp.Match(info.TextSentanceChunk);
            if (match.Success)
            {
                var text = info.TextSentanceChunk.Substring(match.Length);
                var delayStr = match.Groups[4]?.Value;
                if (delayStr != null &&
                    int.TryParse(delayStr, out int delayMs))
                {
                    await Task
                        .Delay(delayMs)
                        .ConfigureAwait(false);
                    await responder
                        .SendMessageAsync(text, originalChatEvent.Space, originalChatEvent.Message.Thread, originalChatEvent.Message.Sender)
                        .ConfigureAwait(false);

                    return null;
                }

                return new ChatEventResult(text);
            }

            return new ChatEventResult("Repeat command can not recognise some segments.");
        }
    }
}
