using System;
using System.Text.Json.Serialization;

using NS = Newtonsoft.Json;

namespace MentorBot.Functions.Models.Business
{
    /// <summary>Google access token info.</summary>
    public sealed class GoogleAccessTokenInfo
    {
        /// <summary>Gets or sets the google user identifier.</summary>
        [NS.JsonProperty(PropertyName = "user_id")]
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        /// <summary>Gets or sets the token expires in seconds from now.</summary>
        [NS.JsonProperty(PropertyName = "expires_in")]
        [JsonPropertyName("expires_in")]
        public int Expires { get; set; }

        /// <summary>Gets or sets the access token type online/offline.</summary>
        [NS.JsonProperty(PropertyName = "access_type")]
        [JsonPropertyName("access_type")]
        public string Type { get; set; }

        /// <summary>Gets or sets the google user default email.</summary>
        [NS.JsonProperty(PropertyName = "email")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>Gets or sets the absolute expiration date at UTC.</summary>
        [NS.JsonIgnore]
        [JsonIgnore]
        public DateTime DueDate { get; set; }
    }
}
