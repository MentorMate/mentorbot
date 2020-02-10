// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.App.Extensions;

namespace MentorBot.Functions.Connectors.Jira
{
    /// <summary>The LUSI web client.</summary>
    public sealed partial class JiraClient : IJiraClient
    {
        /// <summary>The client name.</summary>
        public const string Name = nameof(JiraClient);

        private readonly IHttpClientFactory _clientFactory;

        /// <summary>Initializes a new instance of the <see cref="JiraClient"/> class.</summary>
        public JiraClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        /// <inheritdoc/>
        public async Task<IssuesResponse> QueryAsync(string project, string status, string host, string username, string token)
        {
            var httpClient = _clientFactory.CreateClient(Name);

            httpClient.DefaultRequestHeaders.BasicAuthentication(username, token);

            var httpResponseMessage = await httpClient.GetAsync(
                $"{host.TrimEnd('/')}/rest/api/2/search?jql=project={project} AND status IN (\"{status}\")&maxResults=100&fields=summary,assignee");

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content.ReadAsAsync<IssuesResponse>();
        }
    }
}
