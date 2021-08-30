using MentorBot.Functions.Connectors.BingMaps;
using MentorBot.Functions.Models.Options;
using MentorBot.Tests.Base;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Connectors
{
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class BingMapsClientTests
    {
        [TestMethod]
        public async Task BingMapsClientWouldCallApi()
        {
            var handler = new MockHttpMessageHandler()
                    .Set("{\"resourceSets\": [{\"resources\": [{\"timeZoneAtLocation\": [{\"timeZone\": [{\"genericName\": \"FLE Standard Time\"}]}]}]}]}", "application/json");

            var httpClient = new HttpClient(handler);
            var options = new BingMapsOptions("K");
            var factory = Substitute.For<IHttpClientFactory>();
            var client = new BingMapsClient(factory, options);

            factory.CreateClient("BingMapsClient").Returns(httpClient);

            var result = await client.QueryAsync("Varna");

            Assert.AreEqual("FLE Standard Time", result.GenericName);
        }
    }
}
