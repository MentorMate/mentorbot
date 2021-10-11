using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        private class RawObjectConverter : JsonConverter<object>
        {
            /// <inheritdoc/>
            public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                JsonElement.TryParseValue(ref reader, out var value) ? ToValue(value.Value) : null;

            /// <inheritdoc/>
            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                if (value is int intValue)
                {
                    writer.WriteNumberValue(intValue);
                }
                else if (value is bool boolValue)
                {
                    writer.WriteBooleanValue(boolValue);
                }
                else if (value is string stringValue)
                {
                    writer.WriteStringValue(stringValue);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }

            private static object ToValue(JsonElement element) =>
                element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
                    JsonValueKind.Number => element.GetInt32(),
                    _ => null,
                };
        }
    }
}
