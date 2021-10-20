using System.Text.Json.Serialization;

using MentorBot.Functions.App.JsonConverters;

namespace MentorBot.Functions.Models.Domains.Plugins
{
    /// <summary>A plugin property value.</summary>
    public sealed class PluginPropertyValue
    {
        /// <summary>Gets or sets the key.</summary>
        public string Key { get; set; }

        /// <summary>Gets or sets the value.</summary>
        [JsonConverter(typeof(RawObjectConverter))]
        public object Value { get; set; }
    }
}
