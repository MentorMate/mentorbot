// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
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
        /// <summary>The comman processor name.</summary>
        public const string CommandName = "Help Processor";

        private readonly ILuisClient _luisClient;

        /// <summary>Initializes a new instance of the <see cref="HelpProcessor"/> class.</summary>
        public HelpProcessor(ILuisClient luisClient)
        {
            _luisClient = luisClient;
        }

        /// <inheritdoc/>
        public string Name => CommandName;

        /// <inheritdoc/>
        public string Subject => "Help";

        /// <inheritdoc/>
        public async ValueTask<ChatEventResult> ProcessCommandAsync(TextDeconstructionInformation info, ChatEvent originalChatEvent, IAsyncResponder responder, IReadOnlyDictionary<string, string> settings)
        {
            var examples = await _luisClient.GetExamplesAsync();
            var results = examples.GroupBy(it => it.IntentLabel, (key, group) => group.FirstOrDefault().Text);

            return new ChatEventResult(
                ChatEventFactory.CreateCard(
                    new TextParagraph
                    {
                        Text = string.Join("<br />", results)
                    }));
        }
    }
}
