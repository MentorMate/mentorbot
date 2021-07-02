using System.Text.Json.Serialization;

using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

using NS = Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains.Plugins
{
    /// <summary>A user properties group.</summary>
    public sealed class PluginPropertyGroup
    {
        /// <summary>Gets or sets the name.</summary>
        [NS.JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the unique group name.</summary>
        [NS.JsonProperty("key")]
        [JsonPropertyName("key")]
        public string UniqueName { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="PluginProperty"/> is multi value.</summary>
        [NS.JsonProperty("multi")]
        [JsonPropertyName("multi")]
        public bool Multi { get; set; }

        /// <summary>Gets or sets the property type.</summary>
        [NS.JsonProperty("type")]
        [JsonPropertyName("type")]
        public PropertyObjectTypes ObjectType { get; set; }

        /// <summary>Gets or sets the properties.</summary>
        [StoreAsJsonObject]
        [NS.JsonProperty("properties")]
        [JsonPropertyName("properties")]
        public PluginProperty[] Properties { get; set; }

        /// <summary>Gets or sets the property values.</summary>
        [StoreAsJsonObject]
        [NS.JsonProperty("values")]
        [JsonPropertyName("values")]
        public PluginPropertyValue[][] Values { get; set; }
    }
}
