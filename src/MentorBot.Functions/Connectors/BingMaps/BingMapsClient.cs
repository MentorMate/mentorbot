using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Connectors.BingMaps
{
    /// <summary>The client for Bing Maps.</summary>
    public sealed partial class BingMapsClient : IBingMapsClient
    {
        /// <summary>The client name.</summary>
        public const string Name = nameof(BingMapsClient);

        /// <summary>The bing API v1 url.</summary>
        public const string BingApiV1 = "https://dev.virtualearth.net/REST/v1/";

        private readonly IHttpClientFactory _clientFactory;
        private readonly BingMapsOptions _options;

        /// <summary>Initializes a new instance of the <see cref="BingMapsClient"/> class.</summary>
        public BingMapsClient(
            IHttpClientFactory clientFactory,
            BingMapsOptions options)
        {
            _clientFactory = clientFactory;
            _options = options;
        }

        /// <inheritdoc/>
        public async Task<TimeZoneData> QueryAsync(string location)
        {
            var queryData = HttpUtility.UrlEncode(location);
            using var httpClient = _clientFactory.CreateClient(Name);

            var res = await httpClient.GetAsync(string.Concat(BingApiV1, "timezone/?query=" + queryData, "&key=", _options.BingMapsKey));

            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadAsAsync<QueryResponse>();
            var resource = result.ResourceSets.First().Resources.First();

            return resource.TimeZoneAtLocation.FirstOrDefault()?.TimeZone.FirstOrDefault();
        }
    }
}
