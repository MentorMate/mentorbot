using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.Confluence.ConfluenceClient;

namespace MentorBot.Functions.Connectors.Confluence
{
    /// <summary>The Confluence web client.</summary>
    public interface IConfluenceClient
    {
        /// <summary>Queries the service asynchronous.</summary>
        Task<SearchResponse> QueryAsync(string query, string username, string token);
    }
}
