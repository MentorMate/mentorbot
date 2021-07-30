using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.Jira.JiraClient;

namespace MentorBot.Functions.Connectors.Jira
{
    /// <summary>The JIRA web client.</summary>
    public interface IJiraClient
    {
        /// <summary>Queries the service asynchronous.</summary>
        Task<IssuesResponse> QueryAsync(string project, string status, string host, string username, string token);
    }
}
