// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Connectors.Luis
{
    /// <summary>The LUSI web client.</summary>
    public sealed partial class LuisClient
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

        /// <summary>Queries the service asynchronous.</summary>
        /// <param name="query">The query text.</param>
        public async Task<QueryResponse> QueryAsync(string query)
        {
            var queryData = HttpUtility.UrlEncode(query);
            var url = $"https://{_options.LuisApiHostName}/luis/v2.0/apps/{_options.LuisApiAppId}?timezoneOffset=-360&subscription-key={_options.LuisApiAppKey}&q=" + queryData;
            using (var messageHandler = _messageHandlerFactory())
            {
                using (var client = new HttpClient(messageHandler, false))
                {
                    var response = await client.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsAsync<QueryResponse>();
                }
            }
        }
    }
}
