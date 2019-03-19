using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
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
        public void StorageService_GetNotWorkIfOffLine()
        {
            _documentClientService.IsConnected.Returns(false);

            var address = _service.GetAddresses();
            Assert.AreEqual(0, address.Count);
        }

        [TestMethod]
        public void StorageService_GetAddresses()
        {
            var model = new GoogleAddress { UserName = "A" };
            SetDocumentQuery("mentorbot", "addresses", "SELECT TOP 1000 * FROM addresses", model);

            var result = _service.GetAddresses();

            Assert.AreEqual("A", result[0].UserName);
        }

        [TestMethod]
        public void StorageService_GetMessages()
        {
            var model = new Message { Input = "T" };
            SetDocumentQuery("mentorbot", "messages", "SELECT TOP 1000 m.ProbabilityPercentage FROM messages m", model);

            var result = _service.GetMessages();

            Assert.AreEqual("T", result[0].Input);
        }

        [TestMethod]
        public void StorageService_GetUsersByIdList()
        {
            var model = new User { OpenAirUserId = 10 };
            SetDocumentQuery("mentorbot", "users", "SELECT TOP 1000 * FROM users u WHERE u.OpenAirUserId in (10,15)", model);

            var result = _service.GetUsersByIdList(new[] { 10L, 15L });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(10, result[0].OpenAirUserId);
        }

        [TestMethod]
        public void StorageService_GetAllUsers()
        {
            var model = new User();
            SetDocumentQuery("mentorbot", "users", "SELECT TOP 2000 * FROM users", model);

            var result = _service.GetAllUsers();

            Assert.AreEqual(model, result[0]);
        }

        [TestMethod]
        public void StorageService_AddAddressDoNothingIsNotConnected()
        {
            _documentClientService.IsConnected.Returns(false);
            _documentClientService.DidNotReceive().Get<GoogleAddress>("mentorbot", "addresses");
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
