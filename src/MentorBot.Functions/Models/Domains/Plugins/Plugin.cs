using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

using Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains.Plugins
{
    /// <summary>The plugin domain model.</summary>
    [Storable(nameof(Plugins))]
    public sealed class Plugin
    {
        /// <summary>Gets or sets the identifier.</summary>
        [RowKey]
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>Gets or sets the partition key.</summary>
        [PartitionKey]
        [JsonProperty("key")]
        public string Key { get; set; } = "System";

        /// <summary>Gets or sets the name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the name of the processor type.</summary>
        [JsonProperty("type")]
        public string ProcessorTypeName { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Plugin"/> is enabled.</summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>Gets or sets the properties.</summary>
        [StoreAsJsonObject]
        [JsonProperty("groups")]
        public PluginPropertyGroup[] Groups { get; set; }
    }
}
