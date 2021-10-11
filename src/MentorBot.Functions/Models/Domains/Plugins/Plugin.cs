using System.Text.Json.Serialization;

using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

using NS = Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains.Plugins
{
    /// <summary>The plugin domain model.</summary>
    [Storable(nameof(Plugins))]
    public sealed class Plugin
    {
        /// <summary>Gets or sets the identifier.</summary>
        [RowKey]
        [NS.JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>Gets or sets the partition key.</summary>
        [PartitionKey]
        [NS.JsonProperty("key")]
        [JsonPropertyName("key")]
        public string Key { get; set; } = "System";

        /// <summary>Gets or sets the name.</summary>
        [NS.JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the name of the processor type.</summary>
        [NS.JsonProperty("type")]
        [JsonPropertyName("type")]
        public string ProcessorTypeName { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="Plugin"/> is enabled.</summary>
        [NS.JsonProperty("enabled")]
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>Gets or sets the properties.</summary>
        [StoreAsJsonObject]
        [NS.JsonProperty("groups")]
        [JsonPropertyName("groups")]
        public PluginPropertyGroup[] Groups { get; set; }

        /// <summary>Gets or sets the command examples.</summary>
        [StoreAsJsonObject]
        [NS.JsonProperty("examples")]
        [JsonPropertyName("examples")]
        public string[] Examples { get; set; }
    }
}
