// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

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
                  configuration[nameof(GoogleCreadentialsFilePath)])
        {
        }

        /// <summary>Initializes a new instance of the <see cref="GoogleCloudOptions"/> class.</summary>
        public GoogleCloudOptions(
            string hangoutChatRequestToken,
            string googleCloudApplicationName,
            string googleCloudApiKey,
            string googleCreadentialsFilePath)
        {
            HangoutChatRequestToken = hangoutChatRequestToken;
            GoogleCloudApplicationName = googleCloudApplicationName;
            GoogleCloudApiKey = googleCloudApiKey;
            GoogleCreadentialsFilePath = googleCreadentialsFilePath;
        }

        /// <summary>Gets the security token for Hangout Chat events.</summary>
        public string HangoutChatRequestToken { get; }

        /// <summary>Gets the name of the application.</summary>
        public string GoogleCloudApplicationName { get; }

        /// <summary>Gets the API key.</summary>
        public string GoogleCloudApiKey { get; }

        /// <summary>Gets the google creadentials file path.</summary>
        public string GoogleCreadentialsFilePath { get; }
    }
}
