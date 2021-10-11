using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using MentorBot.Functions.App.Extensions;

namespace MentorBot.Functions.Connectors.Jira
{
    /// <summary>The JIRA web client.</summary>
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
            using var httpClient = _clientFactory.CreateClient(Name);
            var url = new UriBuilder($"{host.TrimEnd('/')}/rest/api/2/search");
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["jql"] = $"project={project} AND status IN (\"{status}\")";
            queryParams["maxResults"] = "100";
            queryParams["fields"] = "summary,assignee";
            url.Port = -1;
            url.Query = queryParams.ToString();

            httpClient.DefaultRequestHeaders.BasicAuthentication(username, token);

            var httpResponseMessage = await httpClient.GetAsync(url.ToString());

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content.ReadAsAsync<IssuesResponse>();
        }
    }
}
