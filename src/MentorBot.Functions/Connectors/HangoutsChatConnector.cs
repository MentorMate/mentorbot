// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1;
using Google.Apis.HangoutsChat.v1.Data;
using Google.Apis.Services;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Options;

using Microsoft.Extensions.Logging;

namespace MentorBot.Functions.Connectors
{
    /// <summary>A hangout chat API connector.</summary>
    /// <seealso cref="IAsyncResponder" />
    public sealed class HangoutsChatConnector : IAsyncResponder
    {
        private readonly ILogger _logger;
        private readonly Lazy<HangoutsChatService> _serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="HangoutsChatConnector"/> class.</summary>
        public HangoutsChatConnector(GoogleCloudOptions options, ILogger log)
            : this(new Lazy<HangoutsChatService>(() => CreateService(options)), log)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HangoutsChatConnector"/> class.</summary>
        public HangoutsChatConnector(Lazy<HangoutsChatService> serviceProvider, ILogger logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        /// ChatEventSpace space, ChatEventMessageThread thread, ChatEventMessageSender sender
        public Task SendMessageAsync(string text, ChatEventSpace space, ChatEventMessageThread thread, ChatEventMessageSender sender)
        {
            if (space == null || sender == null)
            {
                throw new InvalidOperationException("When async responder is called, space and sender are requireired.");
            }

            _logger.LogDebug($"Send a message asynchronius {text} to '{space.DisplayName}' by {sender.DisplayName} '{thread?.Name}'.");
            var messages = _serviceProvider.Value.Spaces.Messages;
            var message = new Message
            {
                Sender = new User
                {
                    Name = sender.Name,
                    DisplayName = sender.DisplayName
                },
                Space = new Space
                {
                    Name = space.Name,
                    DisplayName = space.DisplayName,
                    Type = space.Type
                },
                Text = text
            };

            if (thread != null)
            {
                message.Thread = new Thread
                {
                    Name = thread.Name
                };
            }

            var createRequest = messages.Create(message, space.Name);
            return createRequest?.ExecuteAsync() ?? Task.CompletedTask;
        }

        private static HangoutsChatService CreateService(GoogleCloudOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new HangoutsChatService(
                new BaseClientService.Initializer
                {
                    ApplicationName = options.GoogleCloudApplicationName,
                    ApiKey = options.GoogleCloudApiKey
                });
        }
    }
}
