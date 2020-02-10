// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.Jira.JiraClient;

namespace MentorBot.Functions.Connectors.Jira
{
    /// <summary>The LUSI web client.</summary>
    public interface IJiraClient
    {
        /// <summary>Queries the service asynchronous.</summary>
        Task<IssuesResponse> QueryAsync(string project, string status, string host, string username, string token);
    }
}
