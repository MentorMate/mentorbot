﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using MentorBot.Functions.App.Extensions;

namespace MentorBot.Functions.Connectors.Wikipedia
{
    /// <summary>A Wikipedia api client.</summary>
    public sealed partial class WikiClient : IWikiClient
    {
        private const string WikiApiV1 = "https://en.wikipedia.org/api/rest_v1/page/summary/";
        private readonly Func<HttpMessageHandler> _messageHandlerFactory;

        /// <summary>Initializes a new instance of the <see cref="WikiClient"/> class.</summary>
        public WikiClient()
            : this(() => new HttpClientHandler())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="WikiClient"/> class.</summary>
        public WikiClient(Func<HttpMessageHandler> messageHandlerFactory)
        {
            _messageHandlerFactory = messageHandlerFactory;
        }

        /// <summary>Queries the service asynchronous.</summary>
        /// <param name="query">The query text.</param>
        public async Task<QueryResponse> QueryAsync(string query)
        {
            using var messageHandler = _messageHandlerFactory();
            using var client = new HttpClient(messageHandler, false);

            var url = WikiApiV1 + HttpUtility.UrlEncode(query);
            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<QueryResponse>();
        }
    }
}
