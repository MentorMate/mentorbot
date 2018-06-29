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
        /// <inheritdoc/>
        public IReadOnlyList<TextDeconstructionInformation> InitalializationCommandDefinitians =>
            new[]
            {
                new TextDeconstructionInformation("Repeat", "time", SentenceTypes.Question),
                new TextDeconstructionInformation("Repeat after me", "time", SentenceTypes.Question)
            };

        /// <inheritdoc/>
        public ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder)
        {
            var text = Regex.Replace(info?.TextSentanceChunk, "^repeat (after me)?\\s*", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return new ValueTask<ChatEventResult>(
                new ChatEventResult(text));
        }
    }
}
