using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MentorBot.Functions.App.JsonConverters
{
    /// <summary>A System.TextJson converter that convert value types to raw object and back.</summary>
    public class RawObjectConverter : JsonConverter<object>
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
            else if (value is double doubleValue)
            {
                writer.WriteNumberValue(doubleValue);
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
                JsonValueKind.Number => ToNumber(element),
                _ => null,
            };

        private static object ToNumber(JsonElement element)
        {
            if (element.TryGetInt32(out var intValue))
            {
                return intValue;
            }
            else if (element.TryGetDouble(out var doubleValue))
            {
                return doubleValue;
            }

            return null;
        }
    }
}
