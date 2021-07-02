using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.Extensions.Caching.Memory;

namespace MentorBot.Functions.Services
{
    /// <summary>The most basic implementation of a Cognitive service.</summary>
    public class CognitiveService : ICognitiveService
    {
        private static readonly Regex BotSelfPoint = new Regex(
            "^\\s*@mentorbot\\s*",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

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

            return await GetCognitiveTextAnalysisResultAsync(definition, chatEvent.Message.Sender.Email);
        }

        /// <inheritdoc/>
        public async Task<CognitiveTextAnalysisResult> GetCognitiveTextAnalysisResultAsync(TextDeconstructionInformation definition, string email)
        {
            var plugins = await GetPluginsAsync();
            foreach (var processor in _commandProcessors)
            {
                if (processor.Subject.Equals(definition.Subject, StringComparison.InvariantCultureIgnoreCase))
                {
                    var plugin = plugins.FirstOrDefault(it => it.ProcessorTypeName.Equals(processor.Name, StringComparison.InvariantCulture));
                    if (plugin.Enabled)
                    {
                        var accessor = PluginPropertiesAccessor.GetInstance(email, plugin, _storageService);
                        return new CognitiveTextAnalysisResult(
                            definition,
                            processor,
                            accessor);
                    }
                }
            }

            return null;
        }

        private Task<IReadOnlyList<Plugin>> GetPluginsAsync() =>
            _cache.GetOrCreateAsync(Constants.PluginsCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return _storageService.GetAllPluginsAsync();
            });
    }
}
