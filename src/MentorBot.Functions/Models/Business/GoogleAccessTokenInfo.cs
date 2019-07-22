using System;

using Newtonsoft.Json;

namespace MentorBot.Functions.Models.Business
{
    /// <summary>Google access token info.</summary>
    public sealed class GoogleAccessTokenInfo
    {
        /// <summary>Gets or sets the google user identifier.</summary>
        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }

        /// <summary>Gets or sets the token expires in seconds from now.</summary>
        [JsonProperty(PropertyName = "expires_in")]
        public int Expires { get; set; }

        /// <summary>Gets or sets the access token type online/offline.</summary>
        [JsonProperty(PropertyName = "access_type")]
        public string Type { get; set; }

        /// <summary>Gets or sets the google user default email.</summary>
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        /// <summary>Gets or sets the absolute expiration date at UTC.</summary>
        [JsonIgnore]
        public DateTime DueDate { get; set; }
    }
}
