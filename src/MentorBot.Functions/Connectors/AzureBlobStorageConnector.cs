using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Models.Options;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MentorBot.Functions.Connectors
{
    /// <summary>A blob storage connector that connect to azure cloud storage.</summary>
    /// <seealso cref="IBlobStorageConnector" />
    public class AzureBlobStorageConnector : IBlobStorageConnector
    {
        private readonly CloudStorageAccount _storageAccount;

        /// <summary>Initializes a new instance of the <see cref="AzureBlobStorageConnector"/> class.</summary>
        public AzureBlobStorageConnector(AzureCloudOptions azureCloudOptions)
        {
            IsConnected = CloudStorageAccount.TryParse(azureCloudOptions.AzureStorageAccountConnectionString, out _storageAccount);
        }

        /// <summary>Gets a value indicating whether this instance is connected.</summary>
        public bool IsConnected { get; }

        /// <inheritdoc/>
        public async Task<Stream> GetFileStreamAsync(string path)
        {
            var blockBlob = await GetBlockBlobAsync(path);
            var stream = new MemoryStream();
            await blockBlob.DownloadToStreamAsync(stream);
            return stream;
        }

        /// <summary>Gets a block Blob by a path asynchronous.</summary>
        /// <param name="path">The Blob path.</param>
        [ExcludeFromCodeCoverage]
        public virtual async Task<ICloudBlob> GetBlockBlobAsync(string path)
        {
            var blobPath = BlobPath.ParseAndValidate(path);
            var container = _storageAccount.CreateCloudBlobClient().GetContainerReference(blobPath.ContainerName);
            var exists = await container.ExistsAsync();
            return exists ? container.GetBlockBlobReference(blobPath.FilePath) : throw new DirectoryNotFoundException(@"Container with name {container} do not exists.");
        }

        /// <summary>Holds a connection path to a Blob resource.</summary>
        public struct BlobPath
        {
            /// <summary>Gets the name of the container.</summary>
            public string ContainerName { get; private set; }

            /// <summary>Gets the file path.</summary>
            public string FilePath { get; private set; }

            /// <summary>Parses and validates the path.</summary>
            /// <param name="path">The full path.</param>
            public static BlobPath ParseAndValidate(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    throw new ArgumentNullException(nameof(path));
                }

                var index = path.IndexOf('/');
                if (index <= 0)
                {
                    throw new ArgumentException("Blob path need to be of type container/blobpath.", nameof(path));
                }

                var containerName = path.Substring(0, index);
                var filePath = path.Substring(index + 1);
                if (string.IsNullOrWhiteSpace(containerName) ||
                    string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("Blob path is invalid");
                }

                return new BlobPath
                {
                    ContainerName = containerName,
                    FilePath = filePath
                };
            }
        }
    }
}
