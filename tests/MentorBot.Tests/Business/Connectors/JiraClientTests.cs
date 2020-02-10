using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Connectors.Jira;
using MentorBot.Tests.Base;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Connectors
{
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class JiraClientTests
    {
        [TestMethod]
        public async Task JiraClientWhouldCallApi()
        {
            var handler = new MockHttpMessageHandler()
                    .Set("{ \"total\": 3, \"issues\":[{ \"id\": \"130552\", \"self\": \"https://jira.jj/rest/api/2/issue/130552\", \"key\": \"TT-790\", \"fields\": { \"summary\": \"Do some work\", \"assignee\": { \"name\": \"ali.baba\", \"key\": \"ali.baba\", \"accountId\": \"55\", \"emailAddress\": \"ali.baba@cave.com\", \"displayName\": \"Ali Baba\", } } }] }", "application/json");

            var httpClient = new HttpClient(handler);
            var factory = Substitute.For<IHttpClientFactory>();
            var client = new JiraClient(factory);

            factory.CreateClient("JiraClient").Returns(httpClient);

            var result = await client.QueryAsync("abc", "cde", "https://test.jira.com", "def", "jh");

            Assert.AreEqual(3, result.Total);
            Assert.AreEqual("130552", result.Issues[0].Id);
        }
    }
}
