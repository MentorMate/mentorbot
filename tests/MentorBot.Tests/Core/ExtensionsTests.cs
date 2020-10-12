using System.Collections.Generic;
using System.Net.Http;

using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Core
{
    [TestClass]
    [TestCategory("Core")]
    public sealed class ExtensionsTests
    {
        [TestMethod]
        public void HttpRequestHeadersBasicAuthentication()
        {
            var headers = new HttpClient().DefaultRequestHeaders;
            headers.BasicAuthentication("RR", "AS");
            Assert.AreEqual("Basic", headers.Authorization.Scheme);
            Assert.AreEqual("UlI6QVM=", headers.Authorization.Parameter);
        }

        [TestMethod]
        public void IsManagerWithManagerTheSameUserShouldBeOk()
        {
            IReadOnlyList<User> users = new List<User>
            {
                new User
                {
                    Id = "A",
                    Email = "jhon.doe@abcd.ef",
                    Active = true,
                    Name = "jhon doe",
                    OpenAirUserId = 1,
                    Manager = new UserReference { Email = "jhon.doe@abcd.ef", OpenAirUserId = 1 }
                }
            };

            Assert.IsFalse(users.IsRequestorManager(users[0], "test@test.ts"));
        }
    }
}
