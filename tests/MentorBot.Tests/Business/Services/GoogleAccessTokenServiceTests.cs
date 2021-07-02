using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Services;
using MentorBot.Tests._Base;
using MentorBot.Tests.Base;

using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services
{
    [TestClass]
    [TestCategory("Business.Services")]
    public sealed class GoogleAccessTokenServiceTests
    {
        private IStorageService _storageService;
        private IMemoryCache _cache;
        private MockHttpMessageHandler _messageHandler;
        private GoogleAccessTokenService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _storageService = Substitute.For<IStorageService>();
            _cache = Substitute.For<IMemoryCache>();
            _messageHandler = new MockHttpMessageHandler();
            _service = new GoogleAccessTokenService(_cache, _storageService, () => _messageHandler);
        }

        [TestMethod]
        public async Task ValidateTokenShouldCheckTokenSchema()
        {
            var req = GetRequestWithAuthHeader("Basic ABC123");

            var result = await _service.ValidateTokenAsync(req);

            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public async Task ValidateTokenShouldGetUserFromStore()
        {
            var usr = new User { GoogleUserId = "123", Role = 2 };
            var req = GetRequestWithAuthHeader("Bearer ABC123");

            _messageHandler.Set("{ \"user_id\": \"123\", \"expires_in\":60, \"access_type\":\"online\", \"email\":\"test@domain.com\" }", "application/json");
            _storageService.GetUserByEmailAsync("test@domain.com").Returns(usr);

            var result = await _service.ValidateTokenAsync(req);

            Assert.AreEqual("administrator", result.Role);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public async Task ValidateTokenShouldSaveUserWithouGoogleId()
        {
            var usr = new User { Id = "U1", Role = 1 };
            var req = GetRequestWithAuthHeader("Bearer ABC123");

            _messageHandler.Set("{ \"user_id\": \"123\", \"expires_in\":60, \"access_type\":\"online\", \"email\":\"test@domain.com\" }", "application/json");
            _storageService.GetUserByEmailAsync("test@domain.com").Returns(usr);

            var result = await _service.ValidateTokenAsync(req);

            await _storageService.Received()
                .UpdateUsersAsync(
                    Arg.Is<IReadOnlyList<User>>(it =>
                        it.First().Id == "U1" &&
                        it.First().GoogleUserId == "123"));
        }

        [TestMethod]
        public async Task ValidateTokenShouldInvalidateBasedOnTokenInfo()
        {
            var req = GetRequestWithAuthHeader("Bearer ABC123");

            _messageHandler.Set("{ \"user_id\": \"123\", \"expires_in\":60, \"access_type\":\"offline\", \"email\":\"test@domain.com\" }", "application/json");

            var result = await _service.ValidateTokenAsync(req);

            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public async Task ValidateTokenShouldGetFromCache()
        {
            var req = GetRequestWithAuthHeader("Bearer ABC123");

            _cache.TryGetValue("UserAccessToken_ABC123", out Arg.Any<object>())
                .Returns(x =>
                {
                    x[1] = new GoogleAccessTokenInfo
                    {
                        UserId = "345",
                        Type = "online",
                        Email = "abc@def.go",
                        DueDate = DateTime.UtcNow.AddMinutes(1)
                    };

                    return true;
                });

            _cache.TryGetValue("UserRole_abc@def.go", out Arg.Any<object>())
                .Returns(x =>
                {
                    x[1] = UserRoles.User;
                    return true;
                });


            var result = await _service.ValidateTokenAsync(req);

            Assert.AreEqual(result.Role, "user");
            Assert.IsTrue(result.IsValid);
        }

        private static HttpRequestData GetRequestWithAuthHeader(string value)
        {
            var ctx = MockFunction.GetContext();
            var req = MockFunction.GetRequest(null, ctx);
            var headers = new HttpHeadersCollection();
            headers.Add("Authorization", value);
            req.Headers.Returns(headers);
            return req;
        }
    }
}
