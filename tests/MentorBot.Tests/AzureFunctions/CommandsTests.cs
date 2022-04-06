using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Models.ViewModels;
using MentorBot.Tests._Base;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

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
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(IOpenAirConnector), connector));

            await Commands.SyncUsersAsync(new TimerInfo(), context);

            connector.Received().SyncUsersAsync();
        }

        [TestMethod]
        public async Task ExecuteTimesheetsReminderAsyncShouldWorkOnDivisionByFive()
        {
            var timesheetService = Substitute.For<ITimesheetService>();
            var dateTime = new DateTime(2021, 12, 1, 12, 16, 5, 123, DateTimeKind.Local);
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(ITimesheetService), timesheetService),
                new ServiceDescriptor(typeof(Func<DateTime>), () => dateTime),
                new ServiceDescriptor(typeof(Func<TimeZoneInfo>), () => TimeZoneInfo.Local));

            await Commands.ExecuteTimesheetsReminderAsync(new TimerInfo(), context);

            timesheetService
                .Received()
                .SendScheduledTimesheetNotificationsAsync(
                    new DateTime(2021, 12, 1, 12, 15, 0, 0, DateTimeKind.Local));
        }

        [TestMethod]
        public async Task ExecuteTimesheetsReminderAsyncShouldSendNotifications()
        {
            var timesheetService = Substitute.For<ITimesheetService>();
            var now = DateTime.Now;
            var dateTimeMinutes = (now.Minute / 10) * 10;
            var dateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, dateTimeMinutes, 0, 0, now.Kind);
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(ITimesheetService), timesheetService),
                new ServiceDescriptor(typeof(Func<DateTime>), () => dateTime),
                new ServiceDescriptor(typeof(Func<TimeZoneInfo>), () => TimeZoneInfo.Local));

            await Commands.ExecuteTimesheetsReminderAsync(new TimerInfo(), context);

            timesheetService.Received().SendScheduledTimesheetNotificationsAsync(dateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(AccessViolationException))]
        public async Task SavePluginsAsyncShouldAllowOnlyAdministrators()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var user = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.User };
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService));

            var request = MockFunction.GetRequest(null, context);

            accessTokenService.ValidateTokenAsync(request).Returns(user);

            await Commands.SavePluginsAsync(request, context);
        }

        [TestMethod]
        public async Task SavePluginsAsyncShouldSaveInStorage()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var storageService = Substitute.For<IStorageService>();
            var memoryCache = Substitute.For<IMemoryCache>();
            var user = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.Administrator };
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService),
                new ServiceDescriptor(typeof(IStorageService), storageService),
                new ServiceDescriptor(typeof(IMemoryCache), memoryCache));
            var request = MockFunction.GetRequest(
                "[{\"id\": \"1\", \"key\":\"a\", \"name\":\"A\",\"type\":\"a1\", \"enabled\": true, \"groups\": null }]",
                context);

            accessTokenService.ValidateTokenAsync(request).Returns(user);

            await Commands.SavePluginsAsync(request, context);

            storageService.Received().AddOrUpdatePluginsAsync(Arg.Is<IReadOnlyList<Plugin>>(list => list.Count == 1 && list[0].Id == "1"));
        }

        [TestMethod]
        public async Task SavePluginsAsyncShouldClearCache()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var storageService = Substitute.For<IStorageService>();
            var memoryCache = Substitute.For<IMemoryCache>();
            var user = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.Administrator };
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService),
                new ServiceDescriptor(typeof(IStorageService), storageService),
                new ServiceDescriptor(typeof(IMemoryCache), memoryCache));
            var request = MockFunction.GetRequest("[]", context);

            accessTokenService.ValidateTokenAsync(request).Returns(user);

            await Commands.SavePluginsAsync(request, context);

            memoryCache.Received().Remove("plugins");
        }

        [TestMethod]
        [ExpectedException(typeof(AccessViolationException))]
        public async Task SaveUserPropertiesAsyncShouldAllowOnlyAdministrators()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var user = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.User };
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService));
            var request = MockFunction.GetRequest(null, context);

            accessTokenService.ValidateTokenAsync(request).Returns(user);

            await Commands.SavePluginsAsync(request, context);
        }

        [TestMethod]
        [ExpectedException(typeof(AccessViolationException))]
        public async Task SaveUserPropertiesAsyncShouldOnlyUpdateGroups()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var storageService = Substitute.For<IStorageService>();
            var userInfo = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.User };
            var user = new User { Id = "2", Name = "Original", Properties = null };
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService),
                new ServiceDescriptor(typeof(IStorageService), storageService));
            var request = MockFunction.GetRequest(
                "[{\"id\":\"2\",\"email\":\"a@b.c\",\"name\":\"Test\",\"properties\":{\"p1\":[[\"key\":\"k1\",\"value\":\"v1\"]]}}]",
                context);

            storageService.GetUserByEmailAsync("a@b.c").Returns(user);
            accessTokenService.ValidateTokenAsync(request).Returns(userInfo);

            await Commands.SavePluginsAsync(request, context);

            storageService.Received().AddOrUpdateUserAsync(Arg.Is<User>(u => u.Name == "Original" && u.Properties.Count == 1));
        }

        [TestMethod]
        public async Task SaveQuestionsAsyncShouldUpdateQuestions()
        {
            var storageService = Substitute.For<IStorageService>();

            var data = new[]
            {
                new QuestionAnswerViewModel
                {
                    Id = "1",
                    Title = "Office",
                    IsAnswer = false,
                },
                new QuestionAnswerViewModel
                        {
                            Id = "2",
                            Title = "Accounts",
                            IsAnswer = false,
                        },
            };

            var userInfo = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.Administrator };
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var context = MockFunction.GetContext(
                new ServiceDescriptor(typeof(IAccessTokenService), accessTokenService),
                new ServiceDescriptor(typeof(IStorageService), storageService));
            var request = MockFunction.GetRequest(JsonConvert.SerializeObject(data), context);
            accessTokenService.ValidateTokenAsync(request).Returns(userInfo);

            await Commands.SaveQuestionsAsync(request, context);

            storageService.Received().AddOrUpdateQuestionsAsync(
                Arg.Is<IReadOnlyList<QuestionAnswer>>(
                    questions => questions.Count == 2 && questions[0].Id == "1" && questions[1].Id == "2"));
        }

#pragma warning restore CS4014
    }
}
