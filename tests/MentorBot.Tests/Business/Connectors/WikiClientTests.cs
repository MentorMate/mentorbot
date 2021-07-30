// cSpell:ignore pageid
using System.Threading.Tasks;

using MentorBot.Functions.Connectors.Wikipedia;
using MentorBot.Tests.Base;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Business.Connectors
{
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class WikiClientTests
    {
        [TestMethod]
        public async Task WikiClientWouldCallApi()
        {
            var handler = new MockHttpMessageHandler()
                .Set("{\"type\":\"standard\",\"title\":\"Car\",\"displaytitle\":\"Car\",\"namespace\":{\"id\":0,\"text\":\"\"},\"wikibase_item\":\"Q1420\",\"titles\":{\"canonical\":\"Car\",\"normalized\":\"Car\",\"display\":\"Car\"},\"pageid\":13673345,\"thumbnail\":{\"source\":\"https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/401_Gridlock.jpg/320px-401_Gridlock.jpg\",\"width\":320,\"height\":213},\"originalimage\":{\"source\":\"https://upload.wikimedia.org/wikipedia/commons/5/5d/401_Gridlock.jpg\",\"width\":1600,\"height\":1066},\"lang\":\"en\",\"dir\":\"ltr\",\"revision\":\"910277618\",\"tid\":\"ad9b5030-bbc1-11e9-a6d1-cd50c290ced4\",\"timestamp\":\"2019-08-10T22:53:15Z\",\"description\":\"A wheeled motor vehicle used for transportation\",\"content_urls\":{\"desktop\":{\"page\":\"https://en.wikipedia.org/wiki/Car\",\"revisions\":\"https://en.wikipedia.org/wiki/Car?action=history\",\"edit\":\"https://en.wikipedia.org/wiki/Car?action=edit\",\"talk\":\"https://en.wikipedia.org/wiki/Talk:Car\"},\"mobile\":{\"page\":\"https://en.m.wikipedia.org/wiki/Car\",\"revisions\":\"https://en.m.wikipedia.org/wiki/Special:History/Car\",\"edit\":\"https://en.m.wikipedia.org/wiki/Car?action=edit\",\"talk\":\"https://en.m.wikipedia.org/wiki/Talk:Car\"}},\"api_urls\":{\"summary\":\"https://en.wikipedia.org/api/rest_v1/page/summary/Car\",\"metadata\":\"https://en.wikipedia.org/api/rest_v1/page/metadata/Car\",\"references\":\"https://en.wikipedia.org/api/rest_v1/page/references/Car\",\"media\":\"https://en.wikipedia.org/api/rest_v1/page/media/Car\",\"edit_html\":\"https://en.wikipedia.org/api/rest_v1/page/html/Car\",\"talk_page_html\":\"https://en.wikipedia.org/api/rest_v1/page/html/Talk:Car\"},\"extract\":\"A\",\"extract_html\":\"B\"}", "application/json");
            var client = new WikiClient(() => handler);

            var result = await client.QueryAsync("Car");

            Assert.AreEqual("https://en.wikipedia.org/api/rest_v1/page/summary/Car", handler.Responses[0].Url);
            Assert.AreEqual(result.Type, "standard");
            Assert.AreEqual(result.Title, "Car");
            Assert.AreEqual(result.Displaytitle, "Car");
            Assert.AreEqual(result.PageId, 13673345);
            Assert.AreEqual(result.Thumbnail.Source, "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/401_Gridlock.jpg/320px-401_Gridlock.jpg");
            Assert.AreEqual(result.Originalimage.Source, "https://upload.wikimedia.org/wikipedia/commons/5/5d/401_Gridlock.jpg");
            Assert.AreEqual(result.Lang, "en");
            Assert.AreEqual(result.Description, "A wheeled motor vehicle used for transportation");
            Assert.AreEqual(result.Extract, "A");
            Assert.AreEqual(result.ExtractHtml, "B");
            Assert.AreEqual(result.ContentUrls.Desktop.Page, "https://en.wikipedia.org/wiki/Car");
        }
    }
}
