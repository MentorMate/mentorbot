using System.IO;
using System.Threading.Tasks;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Models.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Blob;
using NSubstitute;

namespace MentorBot.Tests.Business.Connectors
{
    /// <summary>Tests for <see cref="AzureBlobStorageConnector" />.</summary>
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class AzureBlobStorageConnectorTests
    {
#pragma warning disable CS4014

        [TestMethod]
        public async Task GetFileStreamAsync_ShouldReturnMemoryStream()
        {
            var configuration = Substitute.For<IConfiguration>();

            configuration["AzureStorageAccountConnectionString"].Returns("ABC");
            configuration["AzureStorageLocalCacheFolder"].Returns("EFG");

            var connector = new AzureBlobStorageConnectorWrapper(configuration);
            var result = await connector.GetFileStreamAsync("HIJ/KLM");

            Assert.IsInstanceOfType(result, typeof(MemoryStream));
            connector.Blob.Received().DownloadToStreamAsync(result);
        }

#pragma warning restore CS4014

        public class AzureBlobStorageConnectorWrapper : AzureBlobStorageConnector
        {
            public AzureBlobStorageConnectorWrapper(IConfiguration configuration)
                : base(new AzureCloudOptions(configuration))
            {
            }

            public ICloudBlob Blob { get; } = Substitute.For<ICloudBlob>();

            protected override Task<ICloudBlob> GetBlockBlobAsync(string path) =>
                Task.FromResult(Blob);
        }
    }
}
