// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;

using Microsoft.Extensions.Logging;

namespace MentorBot.Functions.Services
{
    /// <summary>The default service handaling 'Hangout Chat' events.</summary>
    /// <seealso cref="IHangoutsChatService"/>
    public class HangoutsChatService : IHangoutsChatService
    {
        private const double ConfidentRatingUnknowThreshold = 0.6;

        private readonly ICognitiveService _cognitiveService;
        private readonly IAsyncResponder _responder;
        private readonly ILogger _logger;

        /// <summary>Initializes a new instance of the <see cref="HangoutsChatService"/> class.</summary>
        public HangoutsChatService(
            ICognitiveService cognitiveService,
            IAsyncResponder responder,
            ILogger logger)
        {
            _cognitiveService = cognitiveService;
            _responder = responder;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Message> BasicAsync(ChatEvent chatEvent)
        {
            var command = await _cognitiveService
                .ProcessAsync(chatEvent)
                .ConfigureAwait(false);

            var fail =
                command == null ||
                command.CommandProcessor == null ||
                command.TextDeconstructionInformation.ConfidenceRating < ConfidentRatingUnknowThreshold;

            if (!fail)
            {
                _logger.LogInformation(
                    $"The command resolved is {command.CommandProcessor.Subject} in {command.CommandProcessor.Name}.");
            }

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
                        .ProcessCommandAsync(command.TextDeconstructionInformation, chatEvent, _responder, command.PropertiesAccessor)
            };

            return result;
        }
    }
}
