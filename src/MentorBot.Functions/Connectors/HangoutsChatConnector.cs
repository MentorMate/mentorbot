// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1;
using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Connectors
{
    /// <summary>A hangout chat API connector.</summary>
    /// <seealso cref="IAsyncResponder" />
    /// <seealso cref="GoogleBaseService{HangoutsChatService}" />
    public sealed class HangoutsChatConnector : GoogleBaseService<HangoutsChatService>, IAsyncResponder
    {
        /// <summary>Initializes a new instance of the <see cref="HangoutsChatConnector"/> class.</summary>
        public HangoutsChatConnector(GoogleCloudOptions options)
            : this(new Lazy<HangoutsChatService>(
                () => new HangoutsChatService(InitByKey(options))))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HangoutsChatConnector"/> class.</summary>
        public HangoutsChatConnector(Lazy<HangoutsChatService> serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        /// ChatEventSpace space, ChatEventMessageThread thread, ChatEventMessageSender sender
        public Task SendMessageAsync(string text, ChatEventSpace space, ChatEventMessageThread thread, ChatEventMessageSender sender)
        {
            if (space == null || sender == null)
            {
                throw new InvalidOperationException("When async responder is called, space and sender are requireired.");
            }

            ////_logger.LogDebug($"Send a message asynchronius {text} to '{space.DisplayName}' by {sender.DisplayName} '{thread?.Name}'.");
            var messages = ServiceProvider.Spaces.Messages;
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
    }
}
