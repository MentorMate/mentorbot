using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Connectors.Jenkins;
using MentorBot.Tests.Base;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Connectors
{
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class JenkinsClientTests
    {
        [TestMethod]
        public async Task JenkinsClientWouldCallApi()
        {
            var handler = new MockHttpMessageHandler()
                    .Set("{ \"DisplayName\": \"A\", \"Result\":\"B\" }", "application/json");

            var httpClient = new HttpClient(handler);
            var factory = Substitute.For<IHttpClientFactory>();
            var client = new JenkinsClient(factory);

            factory.CreateClient("JenkinsClient").Returns(httpClient);

            var result = await client.QueryAsync("abc", "https://test.jenkins.com", "def", "jh");

            Assert.AreEqual("A", result.DisplayName);
            Assert.AreEqual("https://test.jenkins.com/job/abc/lastBuild/api/json?tree=building,description,displayName,result,url,changeSet[items[comment]]", handler.Responses[0].Url);
        }
    }
}
