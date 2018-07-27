// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Core.Abstract.Processor;
using MentorBot.Core.Abstract.Services;
using MentorBot.Core.Models.HangoutsChat;
using MentorBot.Core.Models.TextAnalytics;

namespace MentorBot.Business.Services
{
    /// <summary>The most basic implementation of a Cognitive service.</summary>
    /// <seealso cref="MentorBot.Core.Abstract.Services.ICognitiveService" />
    [ExcludeFromCodeCoverage]
    public class CognitiveService : ICognitiveService
    {
        private static readonly ConcurrentDictionary<TextDeconstructionInformation, ICommandProcessor> StupidMachineLearningPool =
            new ConcurrentDictionary<TextDeconstructionInformation, ICommandProcessor>();

        private readonly IEnumerable<ICommandProcessor> _commandProcessors;

        /// <summary>Initializes a new instance of the <see cref="CognitiveService"/> class.</summary>
        public CognitiveService(IEnumerable<ICommandProcessor> commandProcessors)
        {
            _commandProcessors = commandProcessors;

            var commands = SearchForCommands();
            commands
                .AsParallel()
                .ForAll(kv => StupidMachineLearningPool.TryAdd(kv.Key, kv.Value));
        }

        /// <inheritdoc/>
        public Task<CognitiveTextAnalysisResult> ProcessAsync(ChatEvent chatEvent)
        {
            var text = chatEvent?.Message.Text.Trim() ??
                throw new ArgumentNullException(nameof(chatEvent), "The text message is null.");

            var question =
                text.EndsWith("?", StringComparison.InvariantCulture) ||
                Regex.IsMatch(text, "^(([Ww]hat)|([Ww]here)|([Hh]ow)|([Ww]hy)|([Ww]ho))\\s");

            var definition = new TextDeconstructionInformation(text.TrimEnd('?', '.', '!'), null, question ? SentenceTypes.Question : SentenceTypes.Command, null);

            var command = StupidMachineLearningPool.FirstOrDefault(it =>
                it.Key.SentenceType == definition.SentenceType &&
                text.IndexOf(it.Key.TextSentanceChunk, StringComparison.InvariantCultureIgnoreCase) > -1);

            var result = new CognitiveTextAnalysisResult(definition, command.Value, 1.0);

            return Task.FromResult(result);
        }

        private IReadOnlyList<KeyValuePair<TextDeconstructionInformation, ICommandProcessor>> SearchForCommands() =>
            _commandProcessors
                .Where(it => !StupidMachineLearningPool.Values.Contains(it))
                .SelectMany(it => it
                    .InitalializationCommandDefinitians
                    .Select(cd => new KeyValuePair<TextDeconstructionInformation, ICommandProcessor>(cd, it)))
                .ToArray();
    }
}
