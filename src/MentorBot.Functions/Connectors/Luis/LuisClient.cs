using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Connectors.Luis
{
    /// <summary>The LUIS web client.</summary>
    public sealed partial class LuisClient : ILuisClient
    {
        private readonly Func<HttpMessageHandler> _messageHandlerFactory;
        private readonly AzureCloudOptions _options;

        /// <summary>Initializes a new instance of the <see cref="LuisClient"/> class.</summary>
        public LuisClient(AzureCloudOptions options)
            : this(() => new HttpClientHandler(), options)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LuisClient"/> class.</summary>
        public LuisClient(Func<HttpMessageHandler> messageHandlerFactory, AzureCloudOptions options)
        {
            _messageHandlerFactory = messageHandlerFactory;
            _options = options;
        }

        /// <inheritdoc/>
        public async Task<QueryResponse> QueryAsync(string query)
        {
            var url = new UriBuilder($"https://{_options.LuisApiHostName}/luis/v2.0/apps/{_options.LuisApiAppId}");
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["timezoneOffset"] = "-360";
            queryParams["subscription-key"] = _options.LuisApiAppKey;
            queryParams["q"] = query;
            url.Port = -1;
            url.Query = queryParams.ToString();

            using var messageHandler = _messageHandlerFactory();
            using var client = new HttpClient(messageHandler, false);

            var response = await client.GetAsync(url.ToString());

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<QueryResponse>();
        }

        /// <inheritdoc/>
        public async Task<Utterance[]> GetExamplesAsync()
        {
            var url = new UriBuilder(
                $"https://{_options.LuisApiHostName}/luis/api/v2.0/apps/{_options.LuisApiAppId}/versions/0.1_upgraded/examples");

            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["skip"] = "0";
            queryParams["take"] = "100";
            queryParams["subscription-key"] = _options.LuisApiAppKey;
            url.Port = -1;
            url.Query = queryParams.ToString();

            using var messageHandler = _messageHandlerFactory();
            using var client = new HttpClient(messageHandler, false);

            var response = await client.GetAsync(url.ToString());

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<Utterance[]>();
        }
    }
}
