// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>An abstraction used for command processing.</summary>
    public interface ICommandProcessor
    {
        /// <summary>Gets the subject the proccessor is processing.</summary>
        string Subject { get; }

        /// <summary>Processes the when the cognitive service has chosen this command as most likely.</summary>
        /// <param name="info">The test sentance chink information.</param>
        /// <param name="originalChatEvent">The original chat event.</param>
        /// <param name="responder">The async responder bach to the chat.</param>
        /// <param name="settings">Settigns data from the database.</param>
        /// <returns>The chat result when command is synchronous.</returns>
        ValueTask<ChatEventResult> ProcessCommandAsync(
            TextDeconstructionInformation info,
            ChatEvent originalChatEvent,
            IAsyncResponder responder,
            IReadOnlyDictionary<string, string> settings);
    }
}
