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
        public async Task StorageService_GetNotWorkIfOffLine()
        {
            _documentClientService.IsConnected.Returns(false);

            var address = await _service.GetAddressesAsync();
            Assert.AreEqual(0, address.Count);
        }

        [TestMethod]
        public async Task StorageService_GetAddresses()
        {
            var data = Enumerable.Empty<GoogleAddress>();
            var document = Substitute.For<IDocument<GoogleAddress>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.GetAsync<GoogleAddress>("mentorbot", "addresses").Returns(document);
            document.Query("SELECT TOP 1000 * FROM addresses").Returns(data);

            var result = await _service.GetAddressesAsync();
            Assert.AreEqual(data, result);
        }

        [TestMethod]
        public async Task StorageService_GetUsersByIdList()
        {
            var data = Enumerable.Empty<User>();
            var document = Substitute.For<IDocument<User>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.GetAsync<User>("mentorbot", "users").Returns(document);
            document.Query("SELECT TOP 1000 * FROM users u WHERE u.OpenAirUserId in (10,15)").Returns(data);

            var result = await _service.GetUsersByIdListAsync(new[] { 10L, 15L });
            Assert.AreEqual(data, result);
        }

        [TestMethod]
        public async Task StorageService_GetUsersByEmail()
        {
            var model = new User();
            var document = Substitute.For<IDocument<User>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.GetAsync<User>("mentorbot", "users").Returns(document);
            document.Query("SELECT TOP 1 * FROM users u WHERE u.Email = \"test@mm.com\"").Returns(new[] { model });

            var result = await _service.GetUserByEmailAsync("test@mm.com");
            Assert.AreEqual(model, result);
        }

        [TestMethod]
        public void StorageService_AddAddressDoNothingIsNotConnected()
        {
            _documentClientService.IsConnected.Returns(false);
            _documentClientService.DidNotReceive().GetAsync<GoogleAddress>("mentorbot", "addresses");
        }

        #pragma warning disable CS4014

        [TestMethod]
        public async Task StorageService_AddAddress()
        {
            var model = new GoogleAddress();
            var document = Substitute.For<IDocument<GoogleAddress>>();
            _documentClientService.IsConnected.Returns(true);
            _documentClientService.GetAsync<GoogleAddress>("mentorbot", "addresses").Returns(document);

            await _service.AddAddressAsync(model);

            document.Received().AddAsync(model);
        }

        #pragma warning restore CS4014
    }
}
