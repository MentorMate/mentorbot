using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using MentorBot.Functions.App.Extensions;

namespace MentorBot.Functions.Connectors.Confluence
{
    /// <summary>The Confluence web client.</summary>
    public sealed partial class ConfluenceClient : IConfluenceClient
    {
        private const string AtlassianApiV1 = "https://mentormate.atlassian.net/wiki/rest/api/content/search?cql=(type=page%20and%20title~{0})";
        private readonly Func<HttpMessageHandler> _messageHandlerFactory;

        /// <summary>Initializes a new instance of the <see cref="ConfluenceClient"/> class.</summary>
        public ConfluenceClient()
            : this(() => new HttpClientHandler())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConfluenceClient"/> class.</summary>
        public ConfluenceClient(Func<HttpMessageHandler> messageHandlerFactory)
        {
            _messageHandlerFactory = messageHandlerFactory;
        }

        /// <inheritdoc/>
        public async Task<SearchResponse> QueryAsync(string query, string username, string token)
        {
            using var messageHandler = _messageHandlerFactory();
            using var client = new HttpClient(messageHandler, false);

            var url = string.Format(AtlassianApiV1, query);
            client.DefaultRequestHeaders.BasicAuthentication(username, token);

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<SearchResponse>();
        }
    }
}
