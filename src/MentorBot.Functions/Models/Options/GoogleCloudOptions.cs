using Microsoft.Extensions.Configuration;

namespace MentorBot.Functions.Models.Options
{
    /// <summary>Google cloud configuration options.</summary>
    public class GoogleCloudOptions
    {
        /// <summary>Initializes a new instance of the <see cref="GoogleCloudOptions"/> class.</summary>
        public GoogleCloudOptions(IConfiguration configuration)
            : this(
                configuration[nameof(HangoutChatRequestToken)],
                configuration[nameof(GoogleCloudApplicationName)],
                configuration[nameof(GoogleCloudApiKey)],
                configuration[nameof(GoogleCredentialsFilePath)])
        {
        }

        /// <summary>Initializes a new instance of the <see cref="GoogleCloudOptions"/> class.</summary>
        public GoogleCloudOptions(
            string hangoutChatRequestToken,
            string googleCloudApplicationName,
            string googleCloudApiKey,
            string googleCredentialsFilePath)
        {
            HangoutChatRequestToken = hangoutChatRequestToken;
            GoogleCloudApplicationName = googleCloudApplicationName;
            GoogleCloudApiKey = googleCloudApiKey;
            GoogleCredentialsFilePath = googleCredentialsFilePath;
        }

        /// <summary>Gets the security token for Hangout Chat events.</summary>
        public string HangoutChatRequestToken { get; }

        /// <summary>Gets the name of the application.</summary>
        public string GoogleCloudApplicationName { get; }

        /// <summary>Gets the API key.</summary>
        public string GoogleCloudApiKey { get; }

        /// <summary>Gets the google credentials file path.</summary>
        public string GoogleCredentialsFilePath { get; }
    }
}
