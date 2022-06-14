using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.Timesheets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="TimesheetProcessor" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class TimesheetProcessorTests
    {
        private TimesheetProcessor _processor;
        private IOpenAirConnector _connector;
        private ITimesheetNotifier _timesheetNotifier;

        [TestInitialize]
        public void TestInitialize()
        {
            _connector = Substitute.For<IOpenAirConnector>();
            _timesheetNotifier = Substitute.For<ITimesheetNotifier>();
            _processor = new TimesheetProcessor(_connector, _timesheetNotifier);
        }

        [TestMethod]
        public void OpenAirProcessorSubjectShouldBeTimesheets()
        {
            Assert.AreEqual("Timesheets", _processor.Subject);
            Assert.AreEqual("MentorBot.Functions.Processors.Timesheets.TimesheetProcessor", _processor.Name);
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task OpenAirProcessor_ShouldReturnNoState()
        {
            var info = new TextDeconstructionInformation("Get unsubmitted timesheets", null);
            var accessor = Substitute.For<IPluginPropertiesAccessor>();

            accessor.GetAllPluginPropertyValues<string>(null).ReturnsForAnyArgs(new string[0]);

            // Act
            var result = await _processor.ProcessCommandAsync(info, CreateEvent("a@b.c"), null, accessor) as ChatEventResult;

            Assert.AreEqual("Provide a state of the time sheets, like unsubmitted or unapproved!", result.Text);
        }

        [TestMethod]
        public async Task GetTimesheetsAsyncShouldReturnFromOpenAir()
        {
            var dateTime = DateTime.Now;
            var customers = new[] { "Test Customer" };

            // Act
            await _processor.GetTimesheetsAsync(dateTime, TimesheetStates.Unsubmitted, "a@b.c", true, customers);

            await _connector
                .Received()
                .GetUnsubmittedTimesheetsAsync(dateTime, dateTime.Date, TimesheetStates.Unsubmitted, "a@b.c", true, TimesheetsProperties.UserMaxHours, customers);
        }

        [TestMethod]
        public async Task WhenAskedItShouldGetTimesheets()
        {
            var chat = CreateEvent("a@b.c");
            var accessor = Substitute.For<IPluginPropertiesAccessor>();
            var responder = Substitute.For<IHangoutsChatConnector>();
            var timesheet = new Timesheet
            {
                UserName = "users/B",
                UserEmail = "c@d.e",
                DepartmentName = "F",
                Total = 20
            };

            var timesheets = new [] { timesheet };
            var info = new TextDeconstructionInformation(
                "Get unsubmitted timesheets",
                null,
                SentenceTypes.Unknown,
                new Dictionary<string, string[]> { { "State", new[] { "unsubmitted" } } },
                null,
                1.0);

            _connector
                .GetUnsubmittedTimesheetsAsync(DateTime.MinValue, new DateTime(2020, 1, 1), TimesheetStates.Unsubmitted, null, true, "OpenAir.User.MaxHours", new string[0])
                .ReturnsForAnyArgs(timesheets);

            // Act
            var result = await _processor.ProcessCommandAsync(info, chat, responder, accessor);
            await _processor.NotificationTask;

            // Test
            Assert.AreEqual(null, result.Text);
            _timesheetNotifier
                .Received()
                .SendTimesheetNotificationsToUsersAsync(
                    timesheets,
                    email: "a@b.c",
                    departments: null,
                    notify: false,
                    false,
                    TimesheetStates.Unsubmitted,
                    Arg.Is<GoogleChatAddress>(it => it.Sender == chat.Message.Sender && it.Space == chat.Space),
                    responder);
        }

        [TestMethod]
        public async Task WhenAskedNotifyForTimesheets()
        {
            var date = new DateTime(2000, 1, 1, 1, 1, 1);
            var customers = new[] { "D" };
            var responder = Substitute.For<IHangoutsChatConnector>();
            var address = new GoogleChatAddress("space/B", "MentorBot", null, "A", "Jhon");
            var timesheet = new Timesheet
            {
                UserName = "Jhon",
                UserEmail = "c@d.e",
                DepartmentName = "F",
                Total = 20
            };

            var timesheet2 = new Timesheet
            {
                UserName = "ElA",
                UserEmail = "w@n.m",
                DepartmentName = "F",
                Total = 15
            };

            var departments = new[] { "Department" };
            var timesheets = new[] { timesheet, timesheet2 };
            var userTimesheetCache = new Dictionary<string, IReadOnlyList<Timesheet>>();

            responder.GetPrivateAddress(Arg.Any<IReadOnlyList<string>>()).Returns(new[] { address });

            _connector
                .GetUnsubmittedTimesheetsAsync(
                    date,
                    Arg.Any<DateTime>(),
                    TimesheetStates.Unsubmitted,
                    "a@b.c",
                    true,
                    "OpenAir.User.MaxHours",
                    customers)
                .Returns(timesheets);

            // Act
            await _processor.SendTimesheetNotificationsByKeyAsync(
                date,
                TimesheetStates.Unsubmitted,
                "a@b.c",
                customers,
                departments,
                true,
                null,
                "userKey",
                userTimesheetCache,
                responder);

            // Test
            Assert.AreEqual(timesheets, userTimesheetCache["userKey"]);
            _timesheetNotifier
                .Received()
                .SendTimesheetNotificationsToUsersAsync(
                timesheets,
                "a@b.c",
                departments,
                true,
                false,
                TimesheetStates.Unsubmitted,
                null,
                responder);
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
