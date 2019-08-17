// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.Luis.LuisClient;

namespace MentorBot.Functions.Connectors.Luis
{
    /// <summary>The LUSI web client.</summary>
    public interface ILuisClient
    {
        /// <summary>Queries the service asynchronous.</summary>
        /// <param name="query">The query text.</param>
        Task<QueryResponse> QueryAsync(string query);

        /// <summary>Get examples from the service asynchronous.</summary>
        Task<Utterance[]> GetExamplesAsync();
    }
}
