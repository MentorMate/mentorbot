using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using MentorBot.Functions.App.JsonConverters;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Core
{
    [TestClass]
    [TestCategory("Core")]
    public sealed class RawObjectConverterTests
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        [TestMethod]
        public void RawObjectConverterShouldSerialize()
        {
            Assert.AreEqual(@"{""rawValue"":5}", SerializeRawValue(5));
            Assert.AreEqual(@"{""rawValue"":""test""}", SerializeRawValue("test"));
            Assert.AreEqual(@"{""rawValue"":true}", SerializeRawValue(true));
            Assert.AreEqual(@"{""rawValue"":false}", SerializeRawValue(false));
            Assert.AreEqual(@"{""rawValue"":3.1}", SerializeRawValue(3.1));
            Assert.AreEqual(@"{""rawValue"":null}", SerializeRawValue(DateTime.Now));
        }

        [TestMethod]
        public void RawObjectConverterShouldDeserialize()
        {
            Assert.AreEqual(13, DeserializeRawValue(@"{""rawValue"":13}"));
            Assert.AreEqual("testValue", DeserializeRawValue(@"{""rawValue"":""testValue""}"));
            Assert.AreEqual(true, DeserializeRawValue(@"{""rawValue"":true}"));
            Assert.AreEqual(false, DeserializeRawValue(@"{""rawValue"":false}"));
            Assert.AreEqual(5.4, DeserializeRawValue(@"{""rawValue"":5.4}"));
            Assert.AreEqual(null, DeserializeRawValue(@"{""rawValue"":{}}"));
        }

        private static string SerializeRawValue(object value)
        {
            var rawObject = new RawObjectTestClass { RawValue = value };
            return JsonSerializer.Serialize(rawObject, SerializerOptions);
        }

        private static object DeserializeRawValue(string rawJson) =>
            JsonSerializer.Deserialize<RawObjectTestClass>(rawJson, SerializerOptions)?.RawValue;

        internal class RawObjectTestClass
        {
            /// <summary>Gets or sets the raw value.</summary>
            [JsonConverter(typeof(RawObjectConverter))]
            public object RawValue { get; set; }
        }
    }
}
