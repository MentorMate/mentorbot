// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.Jenkins.JenkinsClient;

namespace MentorBot.Functions.Connectors.Jenkins
{
    /// <summary>The LUSI web client.</summary>
    public interface IJenkinsClient
    {
        /// <summary>Queries the service asynchronous.</summary>
        Task<JobResponse> QueryAsync(string jobName, string host, string username, string token);
    }
}
