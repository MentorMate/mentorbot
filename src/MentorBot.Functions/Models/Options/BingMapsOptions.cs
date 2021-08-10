using Microsoft.Extensions.Configuration;

namespace MentorBot.Functions.Models.Options
{
    /// <summary>Bing maps configuration options.</summary>
    public sealed class BingMapsOptions
    {
        /// <summary>Initializes a new instance of the <see cref="BingMapsOptions"/> class.</summary>
        public BingMapsOptions(IConfiguration configuration)
            : this(configuration[nameof(BingMapsKey)])
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BingMapsOptions"/> class.</summary>
        public BingMapsOptions(string bingMapsKey)
        {
            BingMapsKey = bingMapsKey;
        }

        /// <summary>Gets the maps key.</summary>
        public string BingMapsKey { get; }
    }
}
