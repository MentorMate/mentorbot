using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.TextAnalytics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Caching.Memory;
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
            var cognitiveService = Substitute.For<ICognitiveService>();
            var hangoutsChatConnector = Substitute.For<IHangoutsChatConnector>();
            var propertiesAccessor = Substitute.For<IPluginPropertiesAccessor>();
            var deconstructionInformation = new TextDeconstructionInformation(null, null);
            var analysisResult = new CognitiveTextAnalysisResult(deconstructionInformation, null, propertiesAccessor);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(ITimesheetProcessor), timesheetProcessor),
                new ServiceDescriptor(typeof(ICognitiveService), cognitiveService),
                new ServiceDescriptor(typeof(IHangoutsChatConnector), hangoutsChatConnector));

            cognitiveService.GetCognitiveTextAnalysisResultAsync(null, null).ReturnsForAnyArgs(analysisResult);
            propertiesAccessor
                .GetAllPluginPropertyValues<string>("OpenAir.Filters.Customer")
                .Returns(
                    new[]
                    {
                        "A",
                        "B"
                    });

            propertiesAccessor
                .GetPluginPropertyGroup("OpenAir.Notifications")
                .Returns(
                    new[]
                    {
                        new []
                        {
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.Notifications.Email",
                                Value = "test@domain.com"
                            },
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.Notifications.NotifyByEmail",
                                Value = false
                            },
                            new PluginPropertyValue
                            {
                                Key = "OpenAir.Notifications.DontNotifyManager",
                                Value = false
                            },
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
                false,
                null,
                hangoutsChatConnector);
        }

        [TestMethod]
        public async Task ExecuteTimesheetsReminderAsyncShouldSendNotificaitions()
        {
            var timesheetService = Substitute.For<ITimesheetService>();
            var now = DateTime.Now;
            var dateTimeMinutes = (now.Minute / 10) * 10;
            var dateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, dateTimeMinutes, 0, 0, now.Kind);
            var timeInfo = new TimerInfo(
                null,
                new ScheduleStatus
                {
                    Last = dateTime
                },
                false);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(ITimesheetService), timesheetService));

            await Commands.ExecuteTimesheetsReminderAsync(timeInfo);

            timesheetService.Received().SendScheduledTimesheetNotificationsAsync(dateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(AccessViolationException))]
        public async Task SavePluginsAsyncShouldAllowOnlyAdministrators()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var user = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.User };

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService));

            accessTokenService.ValidateTokenAsync(request).Returns(user);

            await Commands.SavePluginsAsync(request);
        }

        [TestMethod]
        public async Task SavePluginsAsyncShouldSaveInStorage()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var storageService = Substitute.For<IStorageService>();
            var user = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.Administrator };
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.ASCII.GetBytes("[{\"id\": \"1\", \"key\":\"a\", \"name\":\"A\",\"type\":\"a1\", \"enabled\": true, \"groups\": null }]"))
            };

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService),
                new ServiceDescriptor(typeof(IStorageService), storageService));

            accessTokenService.ValidateTokenAsync(request).Returns(user);

            await Commands.SavePluginsAsync(request);

            storageService.Received().AddOrUpdatePluginsAsync(Arg.Is<IReadOnlyList<Plugin>>(list => list.Count == 1 && list[0].Id == "1"));
        }

        [TestMethod]
        public async Task SavePluginsAsyncShouldClearCache()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var memoryCache = Substitute.For<IMemoryCache>();
            var user = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.Administrator };
            var request = new DefaultHttpRequest(new DefaultHttpContext());

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService),
                new ServiceDescriptor(typeof(IMemoryCache), memoryCache));

            accessTokenService.ValidateTokenAsync(request).Returns(user);

            await Commands.SavePluginsAsync(request);

            memoryCache.Received().Remove("plugins");
        }

        [TestMethod]
        [ExpectedException(typeof(AccessViolationException))]
        public async Task SaveUserPropertiesAsyncShouldAllowOnlyAdministrators()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var user = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.User };

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService));

            accessTokenService.ValidateTokenAsync(request).Returns(user);

            await Commands.SavePluginsAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(AccessViolationException))]
        public async Task SaveUserPropertiesAsyncShouldOnlyUpdateGroups()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var storageService = Substitute.For<IStorageService>();
            var userInfo = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.User };
            var user = new User { Id = "2", Name = "Original", Properties = null };
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = new MemoryStream(Encoding.ASCII.GetBytes("[{\"id\": \"2\", \"email\":\"a@b.c\", \"name\":\"Test\", \"properties\": { \"p1\": [[\"key\": \"k1\", \"value\":\"v1\"]] } }]"))
            };

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService),
                new ServiceDescriptor(typeof(IStorageService), storageService));

            storageService.GetUserByEmailAsync("a@b.c").Returns(user);
            accessTokenService.ValidateTokenAsync(request).Returns(userInfo);

            await Commands.SavePluginsAsync(request);

            storageService.Received().AddOrUpdateUserAsync(Arg.Is<User>(u => u.Name == "Original" && u.Properties.Count == 1));
        }

#pragma warning restore CS4014
    }
}
