using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.BingMaps.BingMapsClient;

namespace MentorBot.Functions.Connectors.BingMaps
{
    /// <summary>Bing maps HTTP client.</summary>
    public interface IBingMapsClient
    {
        /// <summary>Queries the timezone information.</summary>
        Task<TimeZoneData> QueryAsync(string location);
    }
}
