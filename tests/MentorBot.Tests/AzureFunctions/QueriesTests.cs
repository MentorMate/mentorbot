using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.AzureFunctions
{
    [TestClass]
    [TestCategory("Functions")]
    public class QueriesTests
    {
        [TestMethod]
        public async Task GetMessagesStatisticsAsyncShouldQueryDocument()
        {
            var req = GetHttpRequest();
            var storageService = Substitute.For<IStorageService>();
            var message1 = new Message { ProbabilityPercentage = 96 };
            var message2 = new Message { ProbabilityPercentage = 82 };

            storageService.GetMessagesAsync().Returns(new[] { message1, message2 });

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                GetAccessTokenServiceDescriptor(req, UserRoles.User));

            var result = await Queries.GetMessagesStatisticsAsync(req);
            var array = result.ToArray();

            Assert.AreEqual(2, array.Length);
            Assert.AreEqual(90, array[0].ProbabilityPercentage);
            Assert.AreEqual(80, array[1].ProbabilityPercentage);
        }

        [TestMethod]
        public async Task GetSettingsShouldQueryTheStorage()
        {
            var req = GetHttpRequest();
            var storageService = Substitute.For<IStorageService>();
            var plugins = new []
            {
                new Plugin
                {
                    Name = "P1"
                }
            };

            storageService.GetAllPluginsAsync().Returns(plugins);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                GetAccessTokenServiceDescriptor(req, UserRoles.Administrator));

            var result = await Queries.GetPluginsAsync(req);

            Assert.AreEqual(result.First().Name, "P1");
        }

        [TestMethod]
        public async Task GetSettingsErrorWhenRequestedFromUserRole()
        {
            var req = GetHttpRequest();

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                GetAccessTokenServiceDescriptor(req, UserRoles.User));

            await Assert.ThrowsExceptionAsync<AccessViolationException>(
                () => Queries.GetPluginsAsync(req));
        }

        [TestMethod]
        public async Task GetUserInfoShouldValidateToken()
        {
            var accessTokenService = Substitute.For<IAccessTokenService>();
            var request = GetHttpRequest();

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                GetAccessTokenServiceDescriptor(request, UserRoles.User));

            var result = await Queries.GetUserInfoAsync(request);

            Assert.AreEqual(result.Role, "user");
        }

        [TestMethod]
        public async Task GetUsersShouldQueryActiveUsers()
        {
            var storageService = Substitute.For<IStorageService>();
            var request = GetHttpRequest();
            var data = new []
            {
                new User
                {
                    Id = "1",
                    Name = "U1",
                    Role = 1,
                }
            };

            storageService.GetAllActiveUsersAsync().Returns(data);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                GetAccessTokenServiceDescriptor(request, UserRoles.Administrator));

            var result = await Queries.GetUsersAsync(request);

            Assert.AreEqual(result.First().Name, "U1");
        }

        [TestMethod]
        public async Task GetUsersShouldErrorWhenRequestedFromUserRole()
        {
            var request = GetHttpRequest();

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                GetAccessTokenServiceDescriptor(request, UserRoles.User));

            await Assert.ThrowsExceptionAsync<AccessViolationException>(
                () => Queries.GetUsersAsync(request));
        }

        [TestMethod]
        public async Task GetPluginsAsyncShouldErrorWhenRequestedFromUserRole()
        {
            var request = GetHttpRequest();

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                GetAccessTokenServiceDescriptor(request, UserRoles.User));

            await Assert.ThrowsExceptionAsync<AccessViolationException>(
                () => Queries.GetPluginsAsync(request));
        }

        [TestMethod]
        public async Task GetPluginsAsyncShouldCreatePluginsWhenNonInStorage()
        {
            var storageService = Substitute.For<IStorageService>();
            var request = GetHttpRequest();

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                GetAccessTokenServiceDescriptor(request, UserRoles.Administrator));

            var result = await Queries.GetPluginsAsync(request);

            Assert.AreEqual(9, result.Count());
            storageService.Received().AddOrUpdatePluginsAsync(Arg.Is<IReadOnlyList<Plugin>>(list => list.Count == 9));
        }

        private static ServiceDescriptor GetAccessTokenServiceDescriptor(HttpRequest req, UserRoles role)
        {
            var tokenService = Substitute.For<IAccessTokenService>();
            var info = new AccessTokenUserInfo { IsValid = true, UserRole = role };
            tokenService.ValidateTokenAsync(req).Returns(info);
            return new ServiceDescriptor(typeof(IAccessTokenService), tokenService);
        }

        private static HttpRequest GetHttpRequest()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            return context.Request;
        }
    }
}
