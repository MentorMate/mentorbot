using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services
{
    [TestClass]
    [TestCategory("Business.Services")]
    public sealed class StorageServiceTests
    {
        private StorageService _service;
        private IDocumentClientService _documentClientService;

        [TestInitialize]
        public void TestInitialize()
        {
            _documentClientService = Substitute.For<IDocumentClientService>();
            _service = new StorageService(_documentClientService);
        }

        [TestMethod]
        public async Task StorageService_GetNotWorkIfOffLine()
        {
            _documentClientService.IsConnected.Returns(false);

            var address = await _service.GetAddressesAsync();
            Assert.AreEqual(0, address.Count);
        }

        [TestMethod]
        public async Task StorageService_GetAddresses()
        {
            var model = new GoogleAddress { UserName = "A" };
            SetDocumentQuery("mentorbot", "addresses", "SELECT TOP 1000 * FROM addresses", model);

            var result = await _service.GetAddressesAsync();

            Assert.AreEqual("A", result[0].UserName);
        }

        [TestMethod]
        public async Task StorageService_GetMessages()
        {
            var model = new Message { Input = "T" };
            SetDocumentQuery("mentorbot", "messages", "SELECT TOP 2000 m.ProbabilityPercentage FROM messages m", model);

            var result = await _service.GetMessagesAsync();

            Assert.AreEqual("T", result[0].Input);
        }

        [TestMethod]
        public async Task StorageService_GetUsersByIdList()
        {
            var model = new User { OpenAirUserId = 10 };
            SetDocumentQuery("mentorbot", "users", "SELECT TOP 2000 * FROM users u WHERE u.Active == 1", model);

            var result = await _service.GetAllActiveUsersAsync();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(10, result[0].OpenAirUserId);
        }

        [TestMethod]
        public async Task StorageService_GetAllUsers()
        {
            var model = new User();
            SetDocumentQuery("mentorbot", "users", "SELECT TOP 2000 * FROM users", model);

            var result = await _service.GetAllUsersAsync();

            Assert.AreEqual(model, result[0]);
        }

        [TestMethod]
        public async Task StorageService_GetUserByEmailShouldReturnFromStorage()
        {
            var model = new User();
            SetDocumentQuery("mentorbot", "users", "SELECT TOP 1 * FROM users u WHERE u.Email == 'jhon.doe@mail.com'", model);

            var result = await _service.GetUserByEmailAsync("jhon.doe@mail.com");

            Assert.AreEqual(model, result);
        }

        [TestMethod]
        public async Task StorageService_GetAllPlugins()
        {
            var model = new Plugin[0];
            SetDocumentQuery("mentorbot", "plugins", "SELECT TOP 2000 * FROM plugins", model);

            var result = await _service.GetAllPluginsAsync();

            Assert.AreEqual(model, result);
        }

        [TestMethod]
        public void StorageService_AddAddressDoNothingIsNotConnected()
        {
            _documentClientService.IsConnected.Returns(false);
            _documentClientService.DidNotReceive().Get<GoogleAddress>("mentorbot", "addresses");
        }

        [TestMethod]
        public async Task StorageService_AddOrUpdatePluginsAsyncShouldCallAddOrUpdate()
        {
            var document = GetDocument<Plugin>("mentorbot", "plugins");

            await _service.AddOrUpdatePluginsAsync(
                new[]
                {
                    new Plugin { Key = "K1" },
                    new Plugin { Key = "K2" },
                    new Plugin { Id = "3",  Key = "K3" },
                });

            document.Received().AddManyAsync(Arg.Is<IReadOnlyList<Plugin>>(list => list.Count == 2)).Wait();
            document.Received().UpdateManyAsync(Arg.Is<IReadOnlyList<Plugin>>(list => list.Count == 1)).Wait();
        }

        [TestMethod]
        public async Task StorageService_SaveMessageAsync()
        {
            var document = GetDocument<Message>("mentorbot", "messages");
            var message = new Message { Input = "In" };

            await _service.SaveMessageAsync(message);

            document.Received().AddOrUpdateAsync(message).Wait();
        }

        [TestMethod]
        public async Task StorageService_AddOrUpdateUserAsync()
        {
            var document = GetDocument<User>("mentorbot", "users");
            var user = new User();

            await _service.AddOrUpdateUserAsync(user);

            document.Received().AddOrUpdateAsync(user);
        }

        [TestMethod]
        public async Task StorageService_GetStatistics()
        {
            var model = new Statistics<TimesheetStatistics> { Id = "TestId" };
            SetDocumentQuery("mentorbot", "statistics", "SELECT TOP 1000 * FROM statistics s WHERE s.Data == '2020-10-01' AND s.Time == '20:00'", model);

            var result = await _service.GetStatisticsAsync<TimesheetStatistics>("2020-10-01", "20:00");

            Assert.AreEqual("TestId", result[0].Id);
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task StorageService_AddAddress()
        {
            var models = new[] { new GoogleAddress() };
            var document = GetDocument<GoogleAddress>("mentorbot", "addresses");

            await _service.AddAddressesAsync(models);

            document.Received().AddManyAsync(models);
        }

        [TestMethod]
        public async Task StorageService_AddUsers()
        {
            var models = new[] { new User() };
            var document = GetDocument<User>("mentorbot", "users");

            await _service.AddUsersAsync(models);

            document.Received().AddManyAsync(models);
        }

        [TestMethod]
        public async Task StorageService_UpdateUsers()
        {
            var models = new[] { new User() };
            var document = GetDocument<User>("mentorbot", "users");

            await _service.UpdateUsersAsync(models);

            document.Received().UpdateManyAsync(models);
        }

#pragma warning restore CS4014

        private void SetDocumentQuery<T>(string db, string collectionName, string query, params T[] models) =>
            GetDocument<T>(db, collectionName).Query(query).Returns(models);

        private IDocument<T> GetDocument<T>(string db, string collectionName)
        {
            var document = Substitute.For<IDocument<T>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.Get<T>(db, collectionName).Returns(document);
            return document;
        }
    }
}
