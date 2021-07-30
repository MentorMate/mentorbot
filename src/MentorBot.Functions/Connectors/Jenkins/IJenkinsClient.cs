using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.Jenkins.JenkinsClient;

namespace MentorBot.Functions.Connectors.Jenkins
{
    /// <summary>The Jenkins web client.</summary>
    public interface IJenkinsClient
    {
        /// <summary>Queries the service asynchronous.</summary>
        Task<JobResponse> QueryAsync(string jobName, string host, string username, string token);
    }
}
