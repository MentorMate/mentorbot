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

#pragma warning disable CS4014

        [TestMethod]
        public async Task SendScheduledTimesheetShoudSendToSpace()
        {
            var date = new DateTime(2020, 7, 1, 20, 0, 0, DateTimeKind.Local);
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
                                Value = "20 Wed"
                            },
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.AutoNotifications.Notify",
                                Value = false
                            },
                        }
                    });

            _hangoutsChatConnector.GetAddressByName("spaces/S1").Returns(address);
            timesheetProcessor
                .GetTimesheetsAsync(Arg.Any<DateTime>(), TimesheetStates.Unsubmitted, "a@b.c", true, excludeCusts)
                .Returns(new[] { timesheet });

            await _timesheetService.SendScheduledTimesheetNotificationsAsync(date);

            timesheetProcessor
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
        public async Task SendScheduledTimesheetShoudeStoreStatistics()
        {
            var date = new DateTime(2020, 7, 1, 20, 0, 0, DateTimeKind.Local);
            var timesheetProcessor = Substitute.For<ITimesheetProcessor>();
            var propertiesAccessor = Substitute.For<IPluginPropertiesAccessor>();
            var deconstructionInformation = new TextDeconstructionInformation(null, null);
            var analysisResult = new CognitiveTextAnalysisResult(deconstructionInformation, timesheetProcessor, propertiesAccessor);
            _cognitiveService.GetCognitiveTextAnalysisResultAsync(null, null).ReturnsForAnyArgs(analysisResult);
            propertiesAccessor
                .GetPluginPropertyGroup("OpenAir.GlobalReport")
                .Returns(
                    new[]
                    {
                        new []
                        {
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.GlobalReport.Cron",
                                Value = "20 Wed"
                            },
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.GlobalReport.Email",
                                Value = "d@e.f"
                            },
                        }
                    });

            timesheetProcessor
                .GetTimesheetsAsync(Arg.Any<DateTime>(), TimesheetStates.Unsubmitted, "d@e.f", true, null)
                .Returns(new[] {
                    new Timesheet
                    {
                        UserEmail = "u@e.c",
                        UserName = "Jhon Doe",
                        DepartmentName = "Account",
                        ManagerName = "ben@ten.com",
                        Total = 5,
                        UtilizationInHours = 10
                    }
                });

            await _timesheetService.SendScheduledTimesheetNotificationsAsync(date);

            _storageService
                .Received()
                .AddOrUpdateStatisticsAsync(Arg.Any<Statistics<TimesheetStatistics[]>>());
        }

#pragma warning restore CS4014

        [TestMethod]
        public void CronCheckShoudAllowManyValues()
        {
            var date = new DateTime(2020, 7, 1, 10, 0, 0, DateTimeKind.Local);

            Assert.IsTrue(TimesheetService.CronCheck("10 Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("10:00 Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("9,10 Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("10:00,9 Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("9,10 Wed,Fri", date));
            Assert.IsTrue(TimesheetService.CronCheck("9,10,11 Wed", date));
            Assert.IsFalse(TimesheetService.CronCheck("9 Wed,Fri", date));
            Assert.IsFalse(TimesheetService.CronCheck("10 Fri", date));
            Assert.IsTrue(TimesheetService.CronCheck("* Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("* *", date));
            Assert.IsTrue(TimesheetService.CronCheck("10 *", date));
            Assert.IsFalse(TimesheetService.CronCheck("* Fri", date));
        }

        [TestMethod]
        public void CronCheckShoudAllowMinutesValues()
        {
            var date = new DateTime(2020, 7, 1, 9, 30, 5, DateTimeKind.Local);

            Assert.IsFalse(TimesheetService.CronCheck("9 Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("9:30 Wed", date));
            Assert.IsTrue(TimesheetService.CronCheck("9,9:30 *", date));
            Assert.IsTrue(TimesheetService.CronCheck("9:00,9:30 *", date));
            Assert.IsFalse(TimesheetService.CronCheck("9:15 *", date));
        }

        [TestMethod]
        public void CronCheckShoudAllowEom()
        {
            var date1 = new DateTime(2020, 10, 30, 9, 0, 0, DateTimeKind.Local);
            var date2 = new DateTime(2020, 10, 31, 9, 0, 0, DateTimeKind.Local);

            Assert.IsFalse(TimesheetService.CronCheck("9 EOM", date1));
            Assert.IsTrue(TimesheetService.CronCheck("9 EOM", date2));
            Assert.IsTrue(TimesheetService.CronCheck("9 Fri,EOM", date1));
            Assert.IsFalse(TimesheetService.CronCheck("9,9:30 Wed,EOM", date1));
            Assert.IsTrue(TimesheetService.CronCheck("9:00 Wed,EOM", date2));
            Assert.IsFalse(TimesheetService.CronCheck("9:30 EOM", date2));
        }
    }
}
