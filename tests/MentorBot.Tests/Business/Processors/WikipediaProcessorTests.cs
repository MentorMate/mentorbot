using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Connectors.Wikipedia;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class WikipediaProcessorTests
    {
        private WikipediaProcessor _processor;
        private IWikiClient _client;

        [TestInitialize]
        public void TestInitialize()
        {
            _client = Substitute.For<IWikiClient>();
            _processor = new WikipediaProcessor(_client);
        }

        [TestMethod]
        public void WikipediaProcessorSubjectShoudBeEncyclopedia()
        {
            Assert.AreEqual(_processor.Subject, "Encyclopedia");
        }

        [TestMethod]
        public async Task WikipediaWhenAskedWithNoQueryShouldReturnMessage()
        {
            var result = await _processor.ProcessCommandAsync(new TextDeconstructionInformation("How to dance", "dance", SentenceTypes.Question, new Dictionary<string, string[]>(), null, 1), null, null, null);

            Assert.AreEqual(result.Text, "I do not know the answer to that!");
        }

        [TestMethod]
        public async Task WikipediaWhenAskedShouldCheckTheApi()
        {
            _client.QueryAsync("Michael Jackson").Returns(
                new WikiClient.QueryResponse
                {
                    Displaytitle = "Michael",
                    Thumbnail = new WikiClient.Image
                    {
                        Source = "dummy"
                    },
                    ContentUrls = new WikiClient.Content
                    {
                        Desktop = new WikiClient.Urls
                        {
                            Page = "https://wikipedia.com/michael"
                        }
                    }
                });

            var result = await _processor.ProcessCommandAsync(
                new TextDeconstructionInformation("Who is Michael Jackson", "dance", SentenceTypes.Question, new Dictionary<string, string[]>(), null, 1), null, null, null);

            Assert.AreEqual(result.Cards[0].Header.Title, "Michael");
            Assert.AreEqual(result.Cards[0].Header.ImageUrl, "dummy");
        }
    }
}
