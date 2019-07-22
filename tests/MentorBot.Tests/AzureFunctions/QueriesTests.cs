﻿using System.Linq;
using System.Threading.Tasks;
using MentorBot.Functions;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
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
            var tokenService = Substitute.For<IAccessTokenService>();
            var storageService = Substitute.For<IStorageService>();
            var info = new AccessTokenUserInfo { IsValid = true, UserRole = UserRoles.User };
            var message1 = new Message { ProbabilityPercentage = 96 };
            var message2 = new Message { ProbabilityPercentage = 82 };

            HttpContext context = new DefaultHttpContext();
            context.Request.Method = "GET";

            tokenService.ValidateTokenAsync(Arg.Any<HttpRequest>()).Returns(info);
            storageService.GetMessagesAsync().Returns(new[] { message1, message2 });

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IStorageService), storageService),
                new ServiceDescriptor(typeof(IAccessTokenService), tokenService));

            var result = await Queries.GetMessagesStatisticsAsync(context.Request);
            var array = result.ToArray();

            Assert.AreEqual(2, array.Length);
            Assert.AreEqual(90, array[0].ProbabilityPercentage);
            Assert.AreEqual(80, array[1].ProbabilityPercentage);
        }
    }
}
