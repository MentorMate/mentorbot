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
            LuisApiHostName = config[nameof(LuisApiHostName)];
            LuisApiAppId = config[nameof(LuisApiAppId)];
            LuisApiAppKey = config[nameof(LuisApiAppKey)];
        }

        /// <summary>Initializes a new instance of the <see cref="AzureCloudOptions"/> class.</summary>
        public AzureCloudOptions(string azureStorageAccountConnectionString, string luisApiHostName, string luisApiAppId, string luisApiAppKey)
        {
            AzureStorageAccountConnectionString = azureStorageAccountConnectionString;
            LuisApiHostName = luisApiHostName;
            LuisApiAppId = luisApiAppId;
            LuisApiAppKey = luisApiAppKey;
        }

        /// <summary>Gets or sets the azure storage account connection string.</summary>
        public string AzureStorageAccountConnectionString { get; set; }

        /// <summary>Gets the azure storage local cache folder.</summary>
        public string AzureStorageLocalCacheFolder => "store_cache";

        /// <summary>Gets or sets the name of the LUIS API host.</summary>
        public string LuisApiHostName { get; set; }

        /// <summary>Gets or sets the LUIS API application identifier.</summary>
        public string LuisApiAppId { get; set; }

        /// <summary>Gets or sets the LUIS API application key.</summary>
        public string LuisApiAppKey { get; set; }
    }
}
