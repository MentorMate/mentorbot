// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using MentorBot.Core;
using MentorBot.Core.Abstract.Services;
using MentorBot.Core.Models.HangoutsChat;

namespace MentorBot.Business.Services
{
    /// <summary>The default service handaling 'Hangout Chat' events.</summary>
    /// <seealso cref="IHangoutsChatService"/>
    public class HangoutsChatService : IHangoutsChatService
    {
        private const double ConfidentRatingUnknowThreshold = 0.6;

        private readonly ICognitiveService _cognitiveService;

        /// <summary>Initializes a new instance of the <see cref="HangoutsChatService"/> class.</summary>
        public HangoutsChatService(ICognitiveService cognitiveService)
        {
            _cognitiveService = cognitiveService;
        }

        /// <inheritdoc/>
        public async Task<ChatEventResult> BasicAsync(ChatEvent chatEvent)
        {
            var command = await _cognitiveService
                .ProcessAsync(chatEvent)
                .ConfigureAwait(false);

            if (command?.CommandProcessor == null ||
                command.ConfidenceRating < ConfidentRatingUnknowThreshold)
            {
                return new ChatEventResult(Messages.UnknownCommandText);
            }

            var result = await command.CommandProcessor.ProcessCommandAsync(command.TextDeconstructionInformation, chatEvent, null);

            return result;
        }
    }
}
