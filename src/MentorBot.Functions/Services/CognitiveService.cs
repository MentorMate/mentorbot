// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Settings;
using MentorBot.Functions.Models.TextAnalytics;

using Microsoft.Extensions.Caching.Memory;

namespace MentorBot.Functions.Services
{
    /// <summary>The most basic implementation of a Cognitive service.</summary>
    public class CognitiveService : ICognitiveService
    {
        private static readonly Regex BotSelfPoint = new Regex("^\\s*@mentorbot\\s*", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        #pragma warning disable CA2213
        private readonly IMemoryCache _cache;
        private readonly IStorageService _storageService;
        private readonly IEnumerable<ICommandProcessor> _commandProcessors;
        private readonly ILanguageUnderstandingConnector _connector;
        #pragma warning restore CA2213

        /// <summary>Initializes a new instance of the <see cref="CognitiveService"/> class.</summary>
        public CognitiveService(
            IMemoryCache cache,
            IStorageService storageService,
            IEnumerable<ICommandProcessor> commandProcessors,
            ILanguageUnderstandingConnector connector)
        {
            _cache = cache;
            _storageService = storageService;
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

            var settings = await GetSettingsAsync();
            foreach (var processor in _commandProcessors)
            {
                if (processor.Subject.Equals(definition.Subject, StringComparison.InvariantCultureIgnoreCase))
                {
                    var name = processor.Name;
                    var configuration = settings.Processors.FirstOrDefault(it => it.Name == name);
                    if (configuration.Enabled)
                    {
                        return new CognitiveTextAnalysisResult(
                            definition,
                            processor,
                            configuration.Data == null ? null : new Dictionary<string, string>(configuration.Data));
                    }
                }
            }

            return null;
        }

        private Task<MentorBotSettings> GetSettingsAsync() =>
            _cache.GetOrCreateAsync(Constants.SettingsCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return _storageService.GetSettingsAsync();
            });
    }
}
