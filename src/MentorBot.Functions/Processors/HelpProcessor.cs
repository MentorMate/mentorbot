using System;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A processor that return help information.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class HelpProcessor : ICommandProcessor
    {
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="HelpProcessor"/> class.</summary>
        public HelpProcessor(IStorageService storageService)
        {
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public string Name => GetType().FullName;

        /// <inheritdoc/>
        public string Subject => "Help";

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IPluginPropertiesAccessor accessor)
        {
            var plugins = await _storageService.GetAllPluginsAsync();
            var results = plugins.SelectMany(it => it.Examples ?? Array.Empty<string>()).DefaultIfEmpty(string.Empty);
            return new ChatEventResult(
                ChatEventFactory.CreateCard(
                    new TextParagraph
                    {
                        Text = string.Join("<br />", results)
                    }));
        }
    }
}
