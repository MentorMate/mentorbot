using System.Text.Json.Serialization;

using NS = Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains.Plugins
{
    /// <summary>A plugin specific properties.</summary>
    public sealed class PluginProperty
    {
        /// <summary>Gets or sets the name.</summary>
        [NS.JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the description.</summary>
        [NS.JsonProperty("desc")]
        [JsonPropertyName("desc")]
        public string DescriptionHtml { get; set; }

        /// <summary>Gets or sets the unique property name.</summary>
        [NS.JsonProperty("key")]
        [JsonPropertyName("key")]
        public string UniqueName { get; set; }

        /// <summary>Gets or sets the property type.</summary>
        [NS.JsonProperty("valueType")]
        [JsonPropertyName("valueType")]
        public PropertyValueTypes ValueType { get; set; }
    }
}
