using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.Business;
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
                    Id = "31235913-2cd7-4bb1-9af8-35efa521bb1d",
                    Name = "Issues/Ticketing"
                }
            };

            storageService.GetAllPluginsAsync().Returns(plugins);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                GetAccessTokenServiceDescriptor(req, UserRoles.Administrator));

            var result = await Queries.GetPluginsAsync(req);

            Assert.IsNotNull(result.FirstOrDefault(it => it.Name == "Issues/Ticketing"));
        }

        [TestMethod]
        public async Task GetSettingsShouldUpdateGroups()
        {
            var req = GetHttpRequest();
            var storageService = Substitute.For<IStorageService>();
            var plugins = new[]
            {
                new Plugin
                {
                    Id = "7239ed4d-5b95-4bdd-be2c-007c281e87e6",
                    Name = "Jenkins Build Info",
                    Enabled = true,
                        Groups = new[]
                        {
                            new PluginPropertyGroup
                            {
                                Name = "Jenkins Hosts",
                                UniqueName = "Jenkins.Hosts",
                            }
                        }
                }
            };

            storageService.GetAllPluginsAsync().Returns(plugins);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                GetAccessTokenServiceDescriptor(req, UserRoles.Administrator));

            var result = await Queries.GetPluginsAsync(req);

            var p = result.FirstOrDefault(it => it.Name == "Jenkins Build Info");

            Assert.AreEqual(2, p.Groups.Length);
            storageService.Received().AddOrUpdatePluginsAsync((Plugin[])result);
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

        [TestMethod]
        public async Task GetTimesheettatisticsAsyncShouldQueryDocument()
        {
            var req = GetHttpRequest();
            var storageService = Substitute.For<IStorageService>();
            var stat1 = new Statistics<TimesheetStatistics[]>
            {
                Date = "2020-08-14",
                Time = "20:00",
                Data = new TimesheetStatistics[]
                {
                    new TimesheetStatistics
                    {
                        DepartmentName = ".NET",
                        State = TimesheetStates.Unsubmitted,
                    },
                    new TimesheetStatistics
                    {
                        DepartmentName = ".NET",
                        State = TimesheetStates.Unsubmitted,
                    },
                    new TimesheetStatistics
                    {
                        DepartmentName = "QA",
                        State = TimesheetStates.Unsubmitted,
                    },
                }
            };
            var stat2 = new Statistics<TimesheetStatistics[]>
            {
                Date = "2020-08-07",
                Time = "20:00",
                Data = new TimesheetStatistics[]
                {
                    new TimesheetStatistics
                    {
                        DepartmentName = ".NET",
                        State = TimesheetStates.Unsubmitted,
                    },
                    new TimesheetStatistics
                    {
                        DepartmentName = "QA",
                        State = TimesheetStates.Unsubmitted,
                    },
                }
            };

            var stats3 = new Statistics<TimesheetStatistics[]>[0];

            storageService.GetStatisticsAsync<TimesheetStatistics[]>(null, null)
                .ReturnsForAnyArgs(new[] { stat1 }, new[] { stat2 }, stats3);

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                GetAccessTokenServiceDescriptor(req, UserRoles.User));

            var result = await Queries.GetTimesheetStatisticsAsync(req);
            var array = result.ToArray();

            Assert.AreEqual(4, array.Length);
            Assert.AreEqual("QA", array[0].Department);
            Assert.AreEqual(1, array[0].Count);
            Assert.AreEqual(".NET", array[1].Department);
            Assert.AreEqual(1, array[1].Count);
            Assert.AreEqual("QA", array[2].Department);
            Assert.AreEqual(1, array[2].Count);
            Assert.AreEqual(".NET", array[3].Department);
            Assert.AreEqual(2, array[3].Count);
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
