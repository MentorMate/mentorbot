using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Connectors.Luis;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Processors
{
    /// <summary>A processor that return help information.</summary>
    /// <seealso cref="ICommandProcessor" />
    public sealed class HelpProcessor : ICommandProcessor
    {
        private readonly ILuisClient _luisClient;

        /// <summary>Initializes a new instance of the <see cref="HelpProcessor"/> class.</summary>
        public HelpProcessor(ILuisClient luisClient)
        {
            _luisClient = luisClient;
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
            var examples = await _luisClient.GetExamplesAsync();
            var results = examples
                .GroupBy(it => it.IntentLabel)
                .SelectMany(group => group.Take(4).Select(it => it.Text));

            return new ChatEventResult(
                ChatEventFactory.CreateCard(
                    new TextParagraph
                    {
                        Text = string.Join("<br />", results)
                    }));
        }
    }
}
