using System;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1;
using Google.Apis.HangoutsChat.v1.Data;
using Google.Apis.Requests;

using MentorBot.Functions.Connectors;
using MentorBot.Functions.Models.Business;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MentorBot.Tests.Business.Connectors
{
    /// <summary>Tests for <see cref="HangoutsChatConnector" />.</summary>
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class HangoutsChatConnectorTests
    {
        [TestMethod]
        public async Task HangoutsChatConnector_CreatesMessage()
        {
            var address = new GoogleChatAddress("S1", "S2", "S3", "U1", "U2");
            var requestCreator = Substitute.For<HangoutsChatConnector.IRequestCreator>();
            var connector = new HangoutsChatConnector(new Lazy<HangoutsChatService>(() => null));

            connector.RequestCreator = requestCreator;

            await connector.SendMessageAsync("A", address);

            requestCreator
                .Received()
                .SpacesMessagesCreate(Arg.Is<Message>(it => it.Text.Equals("A")), "S1");
        }

        [TestMethod]
        public void HangoutsChatConnector_GetAddressSkipError()
        {
            var space1 = new Space { Name = "spaces/A", Type = "DM" };
            var space2 = new Space { Name = "spaces/B", Type = "GM" };
            var space3 = new Space { Name = "spaces/C", Type = "DM" };
            var requestCretor = Substitute.For<HangoutsChatConnector.IRequestCreator>();
            var spacesListRequest = Substitute.For<IClientServiceRequest<ListSpacesResponse>>();
            var spacesListResponse = new ListSpacesResponse { Spaces = new[] { space1, space2, space3 } };
            var membersListRequest = Substitute.For<IClientServiceRequest<ListMembershipsResponse>>();

            requestCretor.SpacesList(null).ReturnsForAnyArgs(spacesListRequest);
            requestCretor.SpacesMembersList(null, null).ReturnsForAnyArgs(membersListRequest);
            spacesListRequest.Execute().Returns(spacesListResponse);
            membersListRequest.Execute().Throws(new Exception());

            var connector = new HangoutsChatConnector(new Lazy<HangoutsChatService>(() => null));

            connector.RequestCreator = requestCretor;

            // Act
            var result = connector.GetPrivateAddress(new[] { "spaces/A" });

            // Test
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("spaces/C", result[0].Space.Name);
            Assert.IsNull(result[0].Sender.Name);
        }

        [TestMethod]
        public void HangoutsChatConnector_GetAddress()
        {
            var space = new Space { Name = "spaces/D", Type = "DM" };
            var membership1 = new Membership { Member = new User { Name = "user/Q", DisplayName = "Q", Type = "HUMAN" } };
            var membership2 = new Membership { Member = new User { Name = "user/W", DisplayName = "W", Type = "BOT" } };
            var requestCretor = Substitute.For<HangoutsChatConnector.IRequestCreator>();
            var spacesListRequest = Substitute.For<IClientServiceRequest<ListSpacesResponse>>();
            var spacesListResponse = new ListSpacesResponse { Spaces = new[] { space } };
            var membersListRequest = Substitute.For<IClientServiceRequest<ListMembershipsResponse>>();
            var membersListResponse = new ListMembershipsResponse { Memberships = new[] { membership1, membership2 } };

            requestCretor.SpacesList(null).ReturnsForAnyArgs(spacesListRequest);
            requestCretor.SpacesMembersList(null, null).ReturnsForAnyArgs(membersListRequest);
            spacesListRequest.Execute().Returns(spacesListResponse);
            membersListRequest.Execute().Returns(membersListResponse);

            var connector = new HangoutsChatConnector(new Lazy<HangoutsChatService>(() => null));

            connector.RequestCreator = requestCretor;

            // Act
            var result2 = connector.GetPrivateAddress(new string[0]);

            // Test
            Assert.AreEqual(1, result2.Count);
            Assert.AreEqual("spaces/D", result2[0].Space.Name);
            Assert.AreEqual("user/Q", result2[0].Sender.Name);
        }
    }
}
