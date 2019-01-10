using System;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1;
using Google.Apis.HangoutsChat.v1.Data;
using Google.Apis.Services;

using MentorBot.Functions.Connectors;
using MentorBot.Functions.Models.HangoutsChat;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Connectors
{
    /// <summary>Tests for <see cref="HangoutsChatConnector" />.</summary>
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class HangoutsChatConnectorTests
    {
        [TestMethod]
        public async Task ChatConnectorCreatesMessage()
        {
            var space = new ChatEventSpace { Name = "S1", DisplayName = "S2" };
            var sender = new ChatEventMessageSender { Name = "U1", DisplayName = "U2" };
            
            var clientService = Substitute.For<IClientService>();
            var service = Substitute.ForPartsOf<HangoutsChatService>();
            var spaces = Substitute.ForPartsOf<SpacesResource>(clientService);
            var messages = Substitute.ForPartsOf<SpacesResource.MessagesResource>(clientService);

            service.Spaces.Returns(spaces);
            spaces.Messages.Returns(messages);
            messages.Create(null, null).ReturnsForAnyArgs((SpacesResource.MessagesResource.CreateRequest)null);
            
            var connector = new HangoutsChatConnector(new Lazy<HangoutsChatService>(service));

            await connector.SendMessageAsync("A", space, null, sender);

            messages.Received().Create(Arg.Any<Message>(), "S1");
        }
    }
}
