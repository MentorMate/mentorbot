// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.IO;

using Microsoft.Extensions.Configuration;

namespace MentorBot.Functions.Models.Options
{
    /// <summary>Google cloud configuration options.</summary>
    public class GoogleCloudOptions
    {
        /// <summary>Initializes a new instance of the <see cref="GoogleCloudOptions"/> class.</summary>
        public GoogleCloudOptions(IConfiguration configuration)
        {
            var config = configuration ?? throw new ArgumentNullException(nameof(configuration));

            HangoutChatRequestToken = config[nameof(HangoutChatRequestToken)];
            GoogleCloudApplicationName = config[nameof(GoogleCloudApplicationName)];
            GoogleCloudApiKey = config[nameof(GoogleCloudApiKey)];
            GoogleCreadentialsFilePath = config[nameof(GoogleCreadentialsFilePath)];
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
