﻿// cSpell:ignore Jhon
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>Tests for <see cref="TimesheetScheduleService" />.</summary>
    [TestClass]
    [TestCategory("Business.Processors")]
    public class TimesheetScheduleServiceTests
    {
        private TimesheetScheduleService _timesheetService;
        private IStorageService _storageService;
        private ICognitiveService _cognitiveService;
        private IHangoutsChatConnector _hangoutsChatConnector;

        [TestInitialize]
        public void TestInitialize()
        {
            _storageService = Substitute.For<IStorageService>();
            _cognitiveService = Substitute.For<ICognitiveService>();
            _hangoutsChatConnector = Substitute.For<IHangoutsChatConnector>();
            _timesheetService = new TimesheetScheduleService(_storageService, _cognitiveService, _hangoutsChatConnector);
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task SendScheduledTimesheetShouldSendToSpace()
        {
            var date = new DateTime(2020, 7, 1, 20, 0, 0, DateTimeKind.Local);
            var timesheetProcessor = Substitute.For<ITimesheetProcessor>();
            var propertiesAccessor = Substitute.For<IPluginPropertiesAccessor>();
            var deconstructionInformation = new TextDeconstructionInformation(null, null);
            var analysisResult = new CognitiveTextAnalysisResult(deconstructionInformation, timesheetProcessor, propertiesAccessor);
            var address = new GoogleChatAddress("S1", "Space 1", "RM", "U1", "User 1");
            var timesheet = new Timesheet { UserEmail = "u@e.c", UserName = "Jhon Doe", DepartmentName = "Account", ManagerName = "ben@ten.com", Total = 5, UtilizationInHours = 10 };
            var excludeCustomers = new[] { "C1" };
            _cognitiveService.GetCognitiveTextAnalysisResultAsync(null, null).ReturnsForAnyArgs(analysisResult);

            propertiesAccessor.GetAllPluginPropertyValues<string>("OpenAir.Filters.Customer").Returns(excludeCustomers);
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
                                Key = "OpenAir.AutoNotifications.Departments",
                                Value = "DA, DB"
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
                .GetTimesheetsAsync(Arg.Any<DateTime>(), TimesheetStates.Unsubmitted, "a@b.c", true, excludeCustomers)
                .Returns(new[] { timesheet });

            await _timesheetService.SendScheduledTimesheetNotificationsAsync(date);

            timesheetProcessor
                .Received()
                .SendTimesheetNotificationsByKeyAsync(
                    date,
                    TimesheetStates.Unsubmitted,
                    "a@b.c",
                    excludeCustomers,
                    Arg.Is<string[]>(it => Enumerable.SequenceEqual(it, new[] { "DA", "DB" })),
                    false,
                    address,
                    "unsubmitted_a@b.c",
                    Arg.Is<Dictionary<string, IReadOnlyList<Timesheet>>>(it => it.Count == 0),
                    _hangoutsChatConnector)
                .Wait();
        }

        [TestMethod]
        public async Task SendScheduledTimesheetShouldSendNotificationsOnly()
        {
            var date = new DateTime(2020, 7, 1, 20, 0, 0, DateTimeKind.Local);
            var timesheetProcessor = Substitute.For<ITimesheetProcessor>();
            var propertiesAccessor = Substitute.For<IPluginPropertiesAccessor>();
            var deconstructionInformation = new TextDeconstructionInformation(null, null);
            var analysisResult = new CognitiveTextAnalysisResult(deconstructionInformation, timesheetProcessor, propertiesAccessor);
            var timesheet = new Timesheet { UserEmail = "u@e.c", UserName = "Jhon Doe", DepartmentName = "Account", ManagerName = "ben@ten.com", Total = 5, UtilizationInHours = 10 };
            var excludeCustomers = new[] { "C1" };
            _cognitiveService.GetCognitiveTextAnalysisResultAsync(null, null).ReturnsForAnyArgs(analysisResult);

            propertiesAccessor.GetAllPluginPropertyValues<string>("OpenAir.Filters.Customer").Returns(excludeCustomers);
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
                                Value = ""
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
                                Value = true
                            },
                        }
                    });

            timesheetProcessor
                .GetTimesheetsAsync(Arg.Any<DateTime>(), TimesheetStates.Unsubmitted, "a@b.c", true, excludeCustomers)
                .Returns(new[] { timesheet });

            await _timesheetService.SendScheduledTimesheetNotificationsAsync(date);

            _hangoutsChatConnector.DidNotReceiveWithAnyArgs().GetAddressByName(null);
            timesheetProcessor
                .Received()
                .SendTimesheetNotificationsByKeyAsync(
                    date,
                    TimesheetStates.Unsubmitted,
                    "a@b.c",
                    excludeCustomers,
                    null,
                    true,
                    address: null,
                    userKey: "unsubmitted_a@b.c",
                    Arg.Is<Dictionary<string, IReadOnlyList<Timesheet>>>(it => it.Count == 0),
                    _hangoutsChatConnector)
                .Wait();
        }

        [TestMethod]
        public async Task SendScheduledTimesheetShouldStoreStatistics()
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
        public void CronCheckShouldAllowManyValues()
        {
            var date = new DateTime(2020, 7, 1, 10, 0, 0, DateTimeKind.Local);

            Assert.IsTrue(TimesheetScheduleService.CronCheck("10 Wed", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("10:00 Wed", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9,10 Wed", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("10:00,9 Wed", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9,10 Wed,Fri", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9,10,11 Wed", date));
            Assert.IsFalse(TimesheetScheduleService.CronCheck("9 Wed,Fri", date));
            Assert.IsFalse(TimesheetScheduleService.CronCheck("10 Fri", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("* Wed", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("* *", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("10 *", date));
            Assert.IsFalse(TimesheetScheduleService.CronCheck("* Fri", date));
        }

        [TestMethod]
        public void CronCheckShouldAllowMinutesValues()
        {
            var date = new DateTime(2020, 7, 1, 9, 30, 5, DateTimeKind.Local);
            var date2 = new DateTime(2021, 11, 5, 17, 45, 0, DateTimeKind.Local);

            Assert.IsFalse(TimesheetScheduleService.CronCheck("9 Wed", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9:30 Wed", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9,9:30 *", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9:00,9:30 *", date));
            Assert.IsFalse(TimesheetScheduleService.CronCheck("9:15 *", date));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("17:45 FRI", date2));
        }

        [TestMethod]
        public void CronCheckShouldAllowEom()
        {
            var date1 = new DateTime(2020, 10, 30, 9, 0, 0, DateTimeKind.Local);
            var date2 = new DateTime(2020, 10, 31, 9, 0, 0, DateTimeKind.Local);

            Assert.IsFalse(TimesheetScheduleService.CronCheck("9 EOM", date1));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9 EOM", date2));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9 Fri,EOM", date1));
            Assert.IsFalse(TimesheetScheduleService.CronCheck("9,9:30 Wed,EOM", date1));
            Assert.IsTrue(TimesheetScheduleService.CronCheck("9:00 Wed,EOM", date2));
            Assert.IsFalse(TimesheetScheduleService.CronCheck("9:30 EOM", date2));
        }
    }
}
