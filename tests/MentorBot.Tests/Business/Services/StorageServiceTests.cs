using System.Linq;
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
            var data = Enumerable.Empty<GoogleAddress>();
            var document = Substitute.For<IDocument<GoogleAddress>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.Get<GoogleAddress>("mentorbot", "addresses").Returns(document);
            document.Query("SELECT TOP 1000 * FROM addresses").Returns(data);

            var result = _service.GetAddresses();
            Assert.AreEqual(data, result);
        }

        [TestMethod]
        public void StorageService_GetUsersByIdList()
        {
            var data = Enumerable.Empty<User>();
            var document = Substitute.For<IDocument<User>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.Get<User>("mentorbot", "users").Returns(document);
            document.Query("SELECT TOP 1000 * FROM users u WHERE u.OpenAirUserId in (10,15)").Returns(data);

            var result = _service.GetUsersByIdList(new[] { 10L, 15L });
            Assert.AreEqual(data, result);
        }

        [TestMethod]
        public void StorageService_GetUsersByEmail()
        {
            var model = new User();
            var document = Substitute.For<IDocument<User>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.Get<User>("mentorbot", "users").Returns(document);
            document.Query("SELECT TOP 1 * FROM users u WHERE u.Email = \"test@mm.com\"").Returns(new[] { model });

            var result = _service.GetUserByEmail("test@mm.com");
            Assert.AreEqual(model, result);
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
            var document = Substitute.For<IDocument<GoogleAddress>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.Get<GoogleAddress>("mentorbot", "addresses").Returns(document);

            await _service.AddAddressesAsync(models);

            document.Received().AddManyAsync(models);
        }

        #pragma warning restore CS4014
    }
}
