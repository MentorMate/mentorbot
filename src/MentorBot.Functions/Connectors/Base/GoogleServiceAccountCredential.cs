using System.IO;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Connectors.Base
{
    /// <summary>Google service account information.</summary>
    public class GoogleServiceAccountCredential
    {
        private readonly IBlobStorageConnector _storageConnector;
        private readonly GoogleCloudOptions _options;

        /// <summary>Initializes a new instance of the <see cref="GoogleServiceAccountCredential"/> class.</summary>
        public GoogleServiceAccountCredential(GoogleCloudOptions options, IBlobStorageConnector storageConnector)
        {
            _storageConnector = storageConnector;
            _options = options;
        }

        /// <summary>Gets the google service account stream.</summary>
        public static byte[] GoogleServiceAccount { get; private set; }

        /// <summary>Gets the name of the application.</summary>
        public string ApplicationName => _options.GoogleCloudApplicationName;

        /// <summary>Gets the service account stream asynchronous.</summary>
        public Task<Stream> GetServiceAccountStreamAsync() =>
            _storageConnector.IsConnected ?
            _storageConnector.GetFileStreamAsync(_options.GoogleCreadentialsFilePath) :
            Task.FromResult(Stream.Null);

        /// <summary>Gets the service account stream.</summary>
        public Stream GetServiceAccountStream()
        {
            if (GoogleServiceAccount == null)
            {
                if (GetServiceAccountStreamAsync().Result is MemoryStream stream)
                {
                    GoogleServiceAccount = stream.ToArray();
                }
                else
                {
                    throw new InvalidDataException("Service account was not returned");
                }
            }

            return new MemoryStream(GoogleServiceAccount);
        }
    }
}
