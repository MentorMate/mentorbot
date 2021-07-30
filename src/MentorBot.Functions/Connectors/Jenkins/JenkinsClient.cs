using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using MentorBot.Functions.App.Extensions;

namespace MentorBot.Functions.Connectors.Jenkins
{
    /// <summary>The Jenkins web client.</summary>
    public sealed partial class JenkinsClient : IJenkinsClient
    {
        /// <summary>The client name.</summary>
        public const string Name = nameof(JenkinsClient);

        private readonly IHttpClientFactory _clientFactory;

        /// <summary>Initializes a new instance of the <see cref="JenkinsClient"/> class.</summary>
        public JenkinsClient(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        /// <inheritdoc/>
        public async Task<JobResponse> QueryAsync(string jobName, string host, string username, string token)
        {
            var encodedJobName = HttpUtility.UrlEncode(jobName);
            var httpClient = _clientFactory.CreateClient(Name);

            httpClient.DefaultRequestHeaders.BasicAuthentication(username, token);

            var httpResponseMessage = await httpClient.GetAsync(
                $"{host.TrimEnd('/')}/job/{encodedJobName}/lastBuild/api/json?tree=building,description,displayName,result,url,changeSet[items[comment]]");

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content.ReadAsAsync<JobResponse>();
        }
    }
}
