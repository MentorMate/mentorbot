// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1;
using Google.Apis.HangoutsChat.v1.Data;
using Google.Apis.Requests;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Connectors
{
    /// <summary>A hangout chat API connector.</summary>
    /// <seealso cref="IAsyncResponder" />
    /// <seealso cref="GoogleBaseService{HangoutsChatService}" />
    public sealed class HangoutsChatConnector :
        GoogleBaseService<HangoutsChatService>,
        IHangoutsChatConnector,
        HangoutsChatConnector.IRequestCreator
    {
        private static readonly string[] Scopes = { "https://www.googleapis.com/auth/chat.bot" };

        /// <summary>Initializes a new instance of the <see cref="HangoutsChatConnector"/> class.</summary>
        public HangoutsChatConnector(GoogleServiceAccountCredential accountCredential)
            : this(new Lazy<HangoutsChatService>(
                () => new HangoutsChatService(
                    InitByServiceAccount(
                        accountCredential.ApplicationName,
                        accountCredential.GetServiceAccountStream(),
                        Scopes))))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HangoutsChatConnector"/> class.</summary>
        public HangoutsChatConnector(Lazy<HangoutsChatService> serviceProvider)
            : base(serviceProvider)
        {
            RequestCreator = this;
        }

        /// <summary>Create a google API base request.</summary>
        public interface IRequestCreator
        {
            /// <summary>Create a Spaces.ListRequest.</summary>
            IClientServiceRequest<ListSpacesResponse> SpacesList(int? pageSize);

            /// <summary>Create a Spaces.Members.ListRequest.</summary>
            IClientServiceRequest<ListMembershipsResponse> SpacesMembersList(string spaceName, int? pageSize);

            /// <summary>Create a new message.</summary>
            IClientServiceRequest<Message> SpacesMessagesCreate(Message message, string spaceName);
        }

        /// <summary>Gets or sets the request creator.</summary>
        public IRequestCreator RequestCreator { get; set; }

        /// <inheritdoc/>
        public Task SendMessageAsync(string text, GoogleChatAddress address, params Card[] cards)
        {
            Contract.Ensures(address != null, "Chat address is required!");
            Contract.Ensures(address.Sender != null, "When async responder is called, sender is required!");
            Contract.Ensures(address.Space != null, "When async responder is called, space is required!");

            var message = new Message
            {
                Sender = new User
                {
                    Name = address.Sender.Name,
                    DisplayName = address.Sender.DisplayName
                },
                Space = new Space
                {
                    Name = address.Space.Name,
                    DisplayName = address.Space.DisplayName,
                    Type = address.Space.Type
                },
                Text = text,
                Cards = new List<Card>(cards)
            };

            if (address.ThreadName != null)
            {
                message.Thread = new Thread
                {
                    Name = address.ThreadName
                };
            }

            var createRequest = RequestCreator.SpacesMessagesCreate(message, address.Space.Name);
            return createRequest?.ExecuteAsync() ?? Task.CompletedTask;
        }

        /// <inheritdoc/>
        public IReadOnlyList<GoogleChatAddress> GetPrivateAddress(IReadOnlyList<string> filterSpaces)
        {
            var res = RequestCreator.SpacesList(1000).Execute();
            var spaces = res.Spaces.Where(it => it.Type == "DM" && !filterSpaces.Contains(it.Name));
            var addresses = new List<GoogleChatAddress>();
            foreach (var space in spaces)
            {
                User user = null;
                try
                {
                    var response = RequestCreator.SpacesMembersList(space.Name, 2).Execute();

                    user = response.Memberships.FirstOrDefault(it => it.Member.Type == "HUMAN")?.Member;
                }
                catch (Exception)
                {
                    // If there is request exception just provide empty user to be stored.
                    user = new User();
                }

                if (user == null)
                {
                    continue;
                }

                var address = new GoogleChatAddress(space.Name, space.DisplayName, space.Type, user.Name, user.DisplayName);

                addresses.Add(address);
            }

            return addresses;
        }

        /// <inheritdoc/>
        public IClientServiceRequest<ListSpacesResponse> SpacesList(int? pageSize) =>
            ServiceProviderFactory.Value.Spaces.List().Setup(it => it.PageSize = pageSize);

        /// <inheritdoc/>
        public IClientServiceRequest<ListMembershipsResponse> SpacesMembersList(string spaceName, int? pageSize) =>
            ServiceProviderFactory.Value.Spaces.Members.List(spaceName).Setup(it => it.PageSize = pageSize);

        /// <inheritdoc/>
        public IClientServiceRequest<Message> SpacesMessagesCreate(Message message, string spaceName) =>
            ServiceProviderFactory.Value.Spaces.Messages.Create(message, spaceName);
    }
}
