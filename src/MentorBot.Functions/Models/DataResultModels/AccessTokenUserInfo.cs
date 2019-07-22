using MentorBot.Functions.Models.Domains;

using Newtonsoft.Json;

namespace MentorBot.Functions.Models.DataResultModels
{
    /// <summary>Get basic auth user info.</summary>
    public sealed class AccessTokenUserInfo
    {
        /// <summary>Gets or sets the user role.</summary>
        [JsonIgnore]
        public UserRoles UserRole { get; set; }

        /// <summary>Gets the user role name as string.</summary>
        public string Role => UserRole.ToString().ToLowerInvariant();

        /// <summary>Gets or sets a value indicating whether [token is valid].</summary>
        public bool IsValid { get; set; }
    }
}
