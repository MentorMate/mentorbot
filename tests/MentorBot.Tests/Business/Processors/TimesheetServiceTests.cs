using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.Timesheets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="TimesheetService" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public class TimesheetServiceTests
    {
        private TimesheetService _timesheetService;
        private IStorageService _storageService;
        private ICognitiveService _cognitiveService;
        private IHangoutsChatConnector _hangoutsChatConnector;

        [TestInitialize]
        public void TestInitialize()
        {
            _storageService = Substitute.For<IStorageService>();
            _cognitiveService = Substitute.For<ICognitiveService>();
            _hangoutsChatConnector = Substitute.For<IHangoutsChatConnector>();
            _timesheetService = new TimesheetService(_storageService, _cognitiveService, _hangoutsChatConnector);
        }

        [TestMethod]
        public async Task SendScheduledTimesheetShoudSendToSpace()
        {
            var timesheetProcessor = Substitute.For<ITimesheetProcessor>();
            var propertiesAccessor = Substitute.For<IPluginPropertiesAccessor>();
            var deconstructionInformation = new TextDeconstructionInformation(null, null);
            var analysisResult = new CognitiveTextAnalysisResult(deconstructionInformation, timesheetProcessor, propertiesAccessor);
            var address = new GoogleChatAddress("S1", "Space 1", "RM", "U1", "User 1");
            var timesheet = new Timesheet { UserEmail = "u@e.c", UserName = "Jhon Doe", DepartmentName = "Account", ManagerName = "ben@ten.com", Total = 5, UtilizationInHours = 10 };
            var excludeCusts = new[] { "C1" };
            _cognitiveService.GetCognitiveTextAnalysisResultAsync(null, null).ReturnsForAnyArgs(analysisResult);

            propertiesAccessor.GetAllPluginPropertyValues<string>("OpenAir.Filters.Customer").Returns(excludeCusts);
            propertiesAccessor
                .GetPluginPropertyGroup("OpenAir.AutoNotifications")
                .Returns(
                    new[]
                    {
                        new []
                        {
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.AutoNotifications.Spaces",
                                Value = "S1"
                            },
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.AutoNotifications.Email",
                                Value = "a@b.c"
                            },
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.AutoNotifications.ReportName",
                                Value = "unsubmitted"
                            },
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.AutoNotifications.Cron",
                                Value = DateTime.Now.ToString("h ddd")
                            },
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.AutoNotifications.Notify",
                                Value = false
                            },
                        }
                    });

            _hangoutsChatConnector.GetAddressByName("S1").Returns(address);
            timesheetProcessor
                .GetTimesheetsAsync(Arg.Any<DateTime>(), TimesheetStates.Unsubmitted, "a@b.c", true, excludeCusts)
                .Returns(new[] { timesheet });

            await _timesheetService.SendScheduledTimesheetNotificationsAsync();

            await _storageService
                .Received()
                .AddOrUpdateStatisticsAsync<TimesheetStatistics[]>(Arg.Any<Statistics<TimesheetStatistics[]>>());
            await timesheetProcessor
                .Received()
                .SendTimesheetNotificationsToUsersAsync(
                    Arg.Is<IReadOnlyList<Timesheet>>(it => it[0].UserName == "Jhon Doe"),
                    "a@b.c",
                    null,
                    false,
                    false,
                    TimesheetStates.Unsubmitted,
                    address,
                    _hangoutsChatConnector);
        }

        [TestMethod]
        public void CronCheckShoudAllowManyValues()
        {
            var date = new DateTime(2020, 7, 1, 10, 30, 0, DateTimeKind.Local);

            Assert.IsTrue(TimesheetService.CronCheck("10 Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("9,10 Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("9,10 Wed,Fri", date));
            Assert.IsTrue(TimesheetService.CronCheck("9,10,11 Wed", date));
            Assert.IsFalse(TimesheetService.CronCheck("9 Wed,Fri", date));
            Assert.IsFalse(TimesheetService.CronCheck("10 Fri", date));
            Assert.IsTrue(TimesheetService.CronCheck("* Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("* *", date));
            Assert.IsTrue(TimesheetService.CronCheck("10 *", date));
            Assert.IsFalse(TimesheetService.CronCheck("* Fri", date));
        }
    }
}
