using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Settings;
using MentorBot.Functions.Processors;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.AzureFunctions
{
    [TestClass]
    [TestCategory("Functions")]
    public sealed class CommandsTests
    {
        #pragma warning disable CS4014

        [TestMethod]
        public async Task SyncUserAsyncShouldExecuteSync()
        {
            var connector = Substitute.For<IOpenAirConnector>();

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IOpenAirConnector), connector));

            await Commands.SyncUsersAsync(new TimerInfo(null, null, false));

            connector.Received().SyncUsersAsync();
        }

        [TestMethod]
        public async Task TimesheetsReminderAsyncShouldCallTimesheetNotify()
        {
            var timesheetProcessor = Substitute.For<ITimesheetProcessor>();
            var storageService = Substitute.For<IStorageService>();
            var hangoutsChatConnector = Substitute.For<IHangoutsChatConnector>();

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(ITimesheetProcessor), timesheetProcessor),
                new ServiceDescriptor(typeof(IStorageService), storageService),
                new ServiceDescriptor(typeof(IHangoutsChatConnector), hangoutsChatConnector));

            storageService.GetSettingsAsync().Returns(
                new MentorBotSettings
                {
                    Processors = new[]
                    {
                        new ProcessorSettings
                        {
                            Name = "OpenAirProcessor",
                            Enabled = true,
                            Data = new []
                            {
                                new KeyValuePair<string, string>(Default.EmailKey, "test@domain.com"),
                                new KeyValuePair<string, string>(Default.DefaultExcludedClientKey, "A;B"),
                                new KeyValuePair<string, string>(Default.NotifyByEmailKey, null)
                            }
                        }
                    }
                });

            await Commands.TimesheetsReminderAsync(new TimerInfo(null, null, false));

            timesheetProcessor.Received().NotifyAsync(
                DateTime.Today,
                TimesheetStates.Unsubmitted,
                "test@domain.com",
                Arg.Is<string[]>(data => data[0] == "A" && data[1] == "B"),
                null,
                true,
                false,
                null,
                hangoutsChatConnector);
        }

#pragma warning restore CS4014
    }
}
