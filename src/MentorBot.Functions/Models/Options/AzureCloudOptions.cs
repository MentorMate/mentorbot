using System;

using Microsoft.Extensions.Configuration;

namespace MentorBot.Functions.Models.Options
{
    /// <summary>The options related to azure cloud services.</summary>
    public sealed class AzureCloudOptions
    {
        /// <summary>Initializes a new instance of the <see cref="AzureCloudOptions"/> class.</summary>
        public AzureCloudOptions(IConfiguration configuration)
        {
            var config = configuration ?? throw new ArgumentNullException(nameof(configuration));

            AzureStorageAccountConnectionString = config["AzureWebJobsStorage"] ?? config["Value.AzureWebJobsStorage"];
        }

        /// <summary>Gets or sets the azure storage account connection string.</summary>
        public string AzureStorageAccountConnectionString { get; set; }

        /// <summary>Gets the azure storage local cache folder.</summary>
        public string AzureStorageLocalCacheFolder => "store_cache";
    }
}
