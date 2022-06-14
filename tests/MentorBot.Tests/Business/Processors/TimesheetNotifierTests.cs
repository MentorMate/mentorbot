// cSpell:ignore Jhon
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Processors.Timesheets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class TimesheetNotifierTests
    {
        private TimesheetNotifier _service;
        private IMailService _mailService;
        private IStorageService _storageService;

        [TestInitialize]
        public void TestInitialize()
        {
            _storageService = Substitute.For<IStorageService>();
            _mailService = Substitute.For<IMailService>();
            _service = new TimesheetNotifier(_storageService, _mailService);
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
                DepartmentName = "Department",
                Total = 20
            };

            var timesheet2 = new Timesheet
            {
                UserName = "ElA",
                UserEmail = "w@n.m",
                DepartmentName = "Department",
                Total = 15
            };

            responder.GetPrivateAddress(Arg.Any<IReadOnlyList<string>>()).Returns(new[] { address });

            _storageService.GetAddressesAsync().Returns(new GoogleAddress[0]);

            // Act
            await _service.SendTimesheetNotificationsToUsersAsync(
                new[] { timesheet, timesheet2 },
                "a@b.c",
                new[] { "Department" },
                true,
                true,
                TimesheetStates.Unsubmitted,
                null,
                responder);

            // Test
            responder
                .Received()
                .SendMessageAsync(
                    "Jhon, You have unsubmitted timesheet. Please, submit your timesheet.",
                    Arg.Is<GoogleChatAddress>(it => it.Space.Name == "space/B"))
                .Wait();

            _mailService
                .Received()
                .SendMailAsync(
                    "Timesheet is pending",
                    ", You have unsubmitted timesheet. Please, submit your timesheet.",
                    Arg.Is<string[]>(it => it[0] == "w@n.m"))
                .Wait();

            _storageService
                .Received()
                .AddAddressesAsync(Arg.Is<IReadOnlyList<GoogleAddress>>(arr => arr[0].SpaceName == "space/B"))
                .Wait();

            _mailService
                .Received()
                .SendMailAsync("Users not notified", "All users with unsubmitted timesheets are notified! Total of 2.<br/><br/><b>The following people where notified by a direct massage or email:<br/><b>Jhon</b><br/><b>ElA</b>", "a@b.c")
                .Wait();
        }

        [TestMethod]
        public async Task SendTimesheetNotificationsToUsersShouldSendAllSubmitted()
        {
            var responder = Substitute.For<IHangoutsChatConnector>();
            var address = new GoogleChatAddress("space/B", "MentorBot", null, "A", "Jhon");
            var responses = new[]
            {
                "<b>If you're reading this, probably MentorBot has encountered internal server error or everybody submitted their timesheets on time. The second one Is highly unlikely, so please review the log.</b>",
                "<b>I spent 2 hours looking for unsubmitted timesheets and I couldn't find any! How should I log this time?</b>",
                "<b>It must be the end of the world, because everyone submitted their timesheet on time!</b>",
            };

            // Act
            await _service.SendTimesheetNotificationsToUsersAsync(
                Array.Empty<Timesheet>(),
                "a@b.c",
                null,
                false,
                false,
                TimesheetStates.Unsubmitted,
                address,
                responder);

            // Test
            responder
                .Received()
                .SendMessageAsync(
                    null,
                    Arg.Is<GoogleChatAddress>(it => it.Space.Name == "space/B"),
                    Arg.Is<Card[]>(cards => Array.IndexOf(responses, cards[0].Sections[0].Widgets[0].TextParagraph.Text) > -1))
                .Wait();
        }
    }
}
