using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.Luis;
using MentorBot.Functions.Models.Options;
using MentorBot.Tests.Base;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Business.Connectors
{
    /// <summary>Tests for <see cref="AzureLuisConnector" />.</summary>
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class AzureLuisConnectorTests
    {
        [TestMethod]
        public async Task AzureLuisConnector_Deconstruct()
        {
            var options = new AzureCloudOptions(null, "A", "B", "C");
            var handler = new MockHttpMessageHandler()
                .Set("{ \"query\": \"Test\", \"topScoringIntent\": { \"intent\": \"A\", \"score\": 0.75,  }, \"intents\": [ { \"intent\": \"A\", \"score\": 0.75 }, { \"intent\": \"None\", \"score\": 0.0168218873 }], \"entities\": [{ \"entity\": \"bob\", \"type\": \"Name\", \"startIndex\": 0, \"endIndex\": 8, \"score\": 0.573899543 }] }", "application/json");

            var client = new LuisClient(() => handler, options);
            var connector = new AzureLuisConnector(client);
            var info = await connector.DeconstructAsync("TT");

            Assert.AreEqual("Test", info.TextSentenceChunk);
            Assert.AreEqual(0.75, info.ConfidenceRating);
            Assert.AreEqual("bob", info.Entities["Name"].FirstOrDefault());
        }

        [TestMethod]
        public async Task AzureLuisClientExamples()
        {
            var options = new AzureCloudOptions(null, "A", "B", "C");
            var handler = new MockHttpMessageHandler()
                .Set("[{ \"id\": 533303694, \"text\": \"good day\", \"intentLabel\": \"Hello\" }]", "application/json");

            var client = new LuisClient(() => handler, options);
            var result = await client.GetExamplesAsync();

            Assert.AreEqual(533303694, result[0].Id);
            Assert.AreEqual("good day", result[0].Text);
            Assert.AreEqual("Hello", result[0].IntentLabel);
        }
    }
}
