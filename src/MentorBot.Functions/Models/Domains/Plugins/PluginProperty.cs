using CoreHelpers.WindowsAzure.Storage.Table.Attributes;

using Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains.Plugins
{
    /// <summary>A plugin specific properties.</summary>
    public sealed class PluginProperty
    {
        /// <summary>Gets or sets the name.</summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>Gets or sets the description.</summary>
        [JsonProperty("desc")]
        public string DescriptionHtml { get; set; }

        /// <summary>Gets or sets the unique property name.</summary>
        [JsonProperty("key")]
        public string UniqueName { get; set; }

        /// <summary>Gets or sets the property type.</summary>
        [JsonProperty("valueType")]
        public PropertyValueTypes ValueType { get; set; }
    }
}
