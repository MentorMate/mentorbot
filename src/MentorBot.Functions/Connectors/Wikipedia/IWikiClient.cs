// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.Wikipedia.WikiClient;

namespace MentorBot.Functions.Connectors.Wikipedia
{
    /// <summary>A Wikipedia api client.</summary>
    public interface IWikiClient
    {
        /// <summary>Query the wikipedia api.</summary>
        /// <param name="query">A search term.</param>
        Task<QueryResponse> QueryAsync(string query);
    }
}
