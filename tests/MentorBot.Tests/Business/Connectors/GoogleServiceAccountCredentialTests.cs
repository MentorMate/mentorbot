using System.IO;
using System.Text;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Models.Options;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Connectors
{
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class GoogleServiceAccountCredentialTests
    {
        private GoogleCloudOptions _options;
        private IBlobStorageConnector _storageConnector;
        private GoogleServiceAccountCredential _accountCredential;

        [TestInitialize]
        public void TestInitialize()
        {
            _options = new GoogleCloudOptions(null, "ABC", null, "QZ/XY");
            _storageConnector = Substitute.For<IBlobStorageConnector>();
            _accountCredential = new GoogleServiceAccountCredential(_options, _storageConnector);
        }

        [TestMethod]
        public async Task GetServiceAccountStreamAsync_ShouldReturnNullIfNotConnected()
        {
            _storageConnector.IsConnected.Returns(false);

            var result = await _accountCredential.GetServiceAccountStreamAsync();

            Assert.AreEqual(Stream.Null, result);
        }

        [TestMethod]
        public void GoogleServiceAccountCredential_ShouldHaveAppName()
        {
            Assert.AreEqual("ABC", _accountCredential.ApplicationName);
        }

        [TestMethod]
        public async Task GetServiceAccountStreamAsync_ShouldReturnTheStreamFromStorageConnector()
        {
            var stream = new MemoryStream(0);
            _storageConnector.IsConnected.Returns(true);
            _storageConnector.GetFileStreamAsync("QZ/XY").Returns(stream);

            var result = await _accountCredential.GetServiceAccountStreamAsync();

            Assert.AreEqual(stream, result);
        }

        [TestMethod]
        public void GetServiceAccountStream_ShouldReturnTheStreamCached()
        {
            var text = Encoding.ASCII.GetBytes("ABCD");
            var stream = new MemoryStream(text);
            _storageConnector.IsConnected.Returns(true);
            _storageConnector.GetFileStreamAsync("QZ/XY").Returns(stream);

            var result = _accountCredential.GetServiceAccountStream();
            var resultText = Encoding.ASCII.GetString((result as MemoryStream).ToArray());
            var cacheText = Encoding.ASCII.GetString(GoogleServiceAccountCredential.GoogleServiceAccount);

            Assert.AreEqual("ABCD", resultText);
            Assert.AreEqual("ABCD", cacheText);
        }
    }
}
