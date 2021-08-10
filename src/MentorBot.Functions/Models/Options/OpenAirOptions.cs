using Microsoft.Extensions.Configuration;

namespace MentorBot.Functions.Models.Options
{
    /// <summary>OpenAir configuration options.</summary>
    public sealed class OpenAirOptions
    {
        /// <summary>Initializes a new instance of the <see cref="OpenAirOptions"/> class.</summary>
        public OpenAirOptions(IConfiguration configuration)
            : this(
                  configuration[nameof(OpenAirUrl)],
                  configuration[nameof(OpenAirCompany)],
                  configuration[nameof(OpenAirApiKey)],
                  configuration[nameof(OpenAirUserName)],
                  configuration[nameof(OpenAirPassword)])
        {
        }

        /// <summary>Initializes a new instance of the <see cref="OpenAirOptions"/> class.</summary>
        public OpenAirOptions(string openAirUrl, string openAirCompany, string openAirApiKey, string openAirUserName, string openAirPassword)
        {
            OpenAirUrl = openAirUrl;
            OpenAirCompany = openAirCompany;
            OpenAirApiKey = openAirApiKey;
            OpenAirUserName = openAirUserName;
            OpenAirPassword = openAirPassword;
        }

        /// <summary>Gets the open air URL.</summary>
        public string OpenAirUrl { get; }

        /// <summary>Gets the open air company.</summary>
        public string OpenAirCompany { get; }

        /// <summary>Gets the open air api key.</summary>
        public string OpenAirApiKey { get; }

        /// <summary>Gets the name of the open air user.</summary>
        public string OpenAirUserName { get; }

        /// <summary>Gets the open air password.</summary>
        public string OpenAirPassword { get; }
    }
}
