using System.Threading.Tasks;

using MentorBot.Functions.Connectors.Luis;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class HelpProcessorTests
    {
        private HelpProcessor _processor;
        private ILuisClient _client;

        [TestInitialize]
        public void TestInitialize()
        {
            _client = Substitute.For<ILuisClient>();
            _processor = new HelpProcessor(_client);
        }

        [TestMethod]
        public void HelpProcessorSubjectShouldBeHelp()
        {
            Assert.AreEqual(_processor.Subject, "Help");
        }

        [TestMethod]
        public void HelpProcessorNameShouldBeOk()
        {
            Assert.AreEqual(_processor.Name, "MentorBot.Functions.Processors.HelpProcessor");
        }

        [TestMethod]
        public async Task HelpWhenAskedShouldCheckTheApi()
        {
            _client.GetExamplesAsync().Returns(
                new[]
                {
                    new LuisClient.Utterance
                    {
                        Text = "1",
                        IntentLabel = "A"
                    },
                    new LuisClient.Utterance
                    {
                        Text = "2",
                        IntentLabel = "A"
                    },
                    new LuisClient.Utterance
                    {
                        Text = "10",
                        IntentLabel = "C"
                    },
                    new LuisClient.Utterance
                    {
                        Text = "3",
                        IntentLabel = "A"
                    },
                    new LuisClient.Utterance
                    {
                        Text = "4",
                        IntentLabel = "A"
                    },
                    new LuisClient.Utterance
                    {
                        Text = "5",
                        IntentLabel = "A"
                    }
                });

            var result = await _processor.ProcessCommandAsync(new TextDeconstructionInformation(null, null), null, null, null);

            Assert.AreEqual(result.Cards[0].Sections[0].Widgets[0].TextParagraph.Text, "1<br />2<br />3<br />4<br />10");
        }
    }
}
