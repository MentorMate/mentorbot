// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Services
{
    /// <summary>The most basic implementation of a Cognitive service.</summary>
    [ExcludeFromCodeCoverage]
    public class CognitiveService : ICognitiveService
    {
        private static readonly Regex BotSelfPoint = new Regex("^\\s*@mentorbot\\s*", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private readonly IEnumerable<ICommandProcessor> _commandProcessors;
        private readonly ILanguageUnderstandingConnector _connector;

        /// <summary>Initializes a new instance of the <see cref="CognitiveService"/> class.</summary>
        public CognitiveService(
            IEnumerable<ICommandProcessor> commandProcessors,
            ILanguageUnderstandingConnector connector)
        {
            _commandProcessors = commandProcessors;
            _connector = connector;
        }

        /// <inheritdoc/>
        public async Task<CognitiveTextAnalysisResult> ProcessAsync(ChatEvent chatEvent)
        {
            var text = chatEvent?.Message.Text ??
                throw new ArgumentNullException(nameof(chatEvent), "The text message is null.");

            text = BotSelfPoint.Replace(text, string.Empty).Trim();

            var query = text.TrimStart(' ').TrimEnd('?', '.', '!');

            var definition = await _connector.DeconstructAsync(query) ??
                new TextDeconstructionInformation(query, string.Empty);

            var commandProcessor = _commandProcessors.FirstOrDefault(it => it.Subject.Equals(definition.Subject, StringComparison.InvariantCultureIgnoreCase));

            return commandProcessor == null ? null : new CognitiveTextAnalysisResult(definition, commandProcessor);
        }
    }
}
