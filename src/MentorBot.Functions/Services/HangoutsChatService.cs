// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;

namespace MentorBot.Functions.Services
{
    /// <summary>The default service handaling 'Hangout Chat' events.</summary>
    /// <seealso cref="IHangoutsChatService"/>
    public class HangoutsChatService : IHangoutsChatService
    {
        private const double ConfidentRatingUnknowThreshold = 0.6;

        private readonly ICognitiveService _cognitiveService;
        private readonly IAsyncResponder _responder;
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="HangoutsChatService"/> class.</summary>
        public HangoutsChatService(ICognitiveService cognitiveService, IAsyncResponder responder, IStorageService storageService)
        {
            _cognitiveService = cognitiveService;
            _responder = responder;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public async Task<Message> BasicAsync(ChatEvent chatEvent)
        {
            var command = await _cognitiveService
                .ProcessAsync(chatEvent)
                .ConfigureAwait(false);

            var settings = await _storageService.GetSettingsAsync();

            var fail =
                command == null ||
                command.CommandProcessor == null ||
                settings == null ||
                !settings.Processors.Any(p => p.Name == command.CommandProcessor.GetType().Name && p.Enabled) ||
                command.TextDeconstructionInformation.ConfidenceRating < ConfidentRatingUnknowThreshold;

            var rating = fail ? 0 : command.TextDeconstructionInformation.ConfidenceRating * 100;
            var result = new Message
            {
                Id = Guid.NewGuid().ToString(),
                Input = chatEvent?.Message?.Text,
                ProbabilityPercentage = (byte)rating,
                Output = fail
                    ? new ChatEventResult(Messages.UnknownCommandText)
                    : await command
                        .CommandProcessor
                        .ProcessCommandAsync(command.TextDeconstructionInformation, chatEvent, _responder)
            };

            return result;
        }
    }
}
