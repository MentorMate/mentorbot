using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

using Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains.Plugins
{
    /// <summary>A user properties group.</summary>
    public sealed class PluginPropertyGroup
    {
        /// <summary>Gets or sets the name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the unique group name.</summary>
        [JsonProperty("key")]
        public string UniqueName { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="PluginProperty"/> is multi value.</summary>
        [JsonProperty("multi")]
        public bool Multi { get; set; }

        /// <summary>Gets or sets the property type.</summary>
        [JsonProperty("type")]
        public PropertyObjectTypes ObjectType { get; set; }

        /// <summary>Gets or sets the properties.</summary>
        [StoreAsJsonObject]
        [JsonProperty("properties")]
        public PluginProperty[] Properties { get; set; }

        /// <summary>Gets or sets the property values.</summary>
        [StoreAsJsonObject]
        [JsonProperty("values")]
        public PluginPropertyValue[][] Values { get; set; }
    }
}
