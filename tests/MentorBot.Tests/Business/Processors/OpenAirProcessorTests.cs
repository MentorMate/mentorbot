using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

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
        private IMailService _mailService;
        private IStorageService _storageService;

        [TestInitialize]
        public void TestInitialize()
        {
            _connector = Substitute.For<IOpenAirConnector>();
            _storageService = Substitute.For<IStorageService>();
            _mailService = Substitute.For<IMailService>();
            _processor = new OpenAirProcessor(_connector, _storageService, _mailService);
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task OpenAirProcessor_ShouldReturnNoState()
        {
            var info = new TextDeconstructionInformation("Get unsubmitted timesheets", null);
            
            // Act
            var result = await _processor.ProcessCommandAsync(info, CreateEvent("a@b.c"), null, new Dictionary<string, string>()) as ChatEventResult;

            Assert.AreEqual("Provide a state of the time sheets, like unsubmitted or unapproved!", result.Text);
        }

        [TestMethod]
        public async Task WhenAskedItShouldGetTimesheets()
        {
            var chat = CreateEvent("a@b.c");
            var responder = Substitute.For<IHangoutsChatConnector>();
            var timesheet = new Timesheet
            {
                Name = "A",
                UserName = "users/B",
                UserEmail = "c@d.e",
                DepartmentName = "F", Total = 20
            };

            var info = new TextDeconstructionInformation(
                "Get unsubmited timesheets",
                null,
                SentenceTypes.Unknown,
                new Dictionary<string, string[]> { { "State", new[] { "unsubmitted" } } },
                null,
                1.0);

            _connector.GetUnsubmittedTimesheetsAsync(DateTime.MinValue, TimesheetStates.Unsubmitted, null, null).ReturnsForAnyArgs(new[] { timesheet });

            // Act
            var result = await _processor.ProcessCommandAsync(info, chat, responder, new Dictionary<string, string>());

            // Test
            System.Threading.Thread.Sleep(150);

            Assert.AreEqual(null, result.Text);
            responder
                .Received()
                .SendMessageAsync(
                    null,
                    Arg.Is<GoogleChatAddress>(it => it.Sender == chat.Message.Sender && it.Space == chat.Space),
                    Arg.Any<Card[]>());
        }

        #pragma warning restore CS4014

        private static ChatEvent CreateEvent(string senderEmail)
        {
            var sender = new ChatEventMessageSender() { Email = senderEmail };
            var space = new ChatEventSpace();
            var message = new ChatEventMessage { Sender = sender };
            return new ChatEvent { Space = space, Message = message };
        }
    }
}
