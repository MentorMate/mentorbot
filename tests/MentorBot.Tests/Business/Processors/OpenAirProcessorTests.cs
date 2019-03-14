using System;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="RepeatProcessor" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class OpenAirProcessorTests
    {
        private OpenAirProcessor _processor;
        private IOpenAirConnector _connector;
        private IStorageService _storageService;

        [TestInitialize]
        public void TestInitialize()
        {
            _connector = Substitute.For<IOpenAirConnector>();
            _storageService = Substitute.For<IStorageService>();
            _processor = new OpenAirProcessor(_connector, _storageService);
        }

        #pragma warning disable CS4014

        [TestMethod]
        public async Task WhenAskedItShouldGetTimesheets()
        {
            var sender = new ChatEventMessageSender() { Email = "a@b.c" };
            var space = new ChatEventSpace();
            var message = new ChatEventMessage { Sender = sender };
            var chat = new ChatEvent { Space = space, Message = message };
            var responder = Substitute.For<IHangoutsChatConnector>();
            var timesheet = new Timesheet { Name = "A", UserName = "users/B", UserEmail = "c@d.e", DepartmentOwnerEmail = "a@b.c", DepartmentName = "F", Total = 20 };
            var info = new TextDeconstructionInformation("Get unsubmited timesheets", null);

            _connector.GetUnsubmittedTimesheetsAsync(DateTime.MinValue, null).ReturnsForAnyArgs(new[] { timesheet });

            // Act
            var result = await _processor.ProcessCommandAsync(info, chat, responder);

            // Test
            System.Threading.Thread.Sleep(150);

            Assert.AreEqual(null, result.Text);
            responder
                .Received()
                .SendMessageAsync(
                    null,
                    Arg.Is<GoogleChatAddress>(it => it.Sender == sender && it.Space == space),
                    Arg.Any<Card[]>());
        }

        #pragma warning restore CS4014
    }
}
