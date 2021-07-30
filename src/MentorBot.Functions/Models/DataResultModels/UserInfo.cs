using System.Collections.Generic;

using MentorBot.Functions.Models.Domains.Plugins;

namespace MentorBot.Functions.Models.DataResultModels
{
    /// <summary>A generic user information.</summary>
    public sealed class UserInfo
    {
        /// <summary>Gets or sets the user identifier.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the email.</summary>
        public string Email { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the user role.</summary>
        public string Role { get; set; }

        /// <summary>Gets or sets the direct manager name.</summary>
        public string Manager { get; set; }

        /// <summary>Gets or sets the department name.</summary>
        public string Department { get; set; }

        /// <summary>Gets or sets the customers.</summary>
        public string Customers { get; set; }

        /// <summary>Gets or sets the properties.</summary>
        public Dictionary<string, PluginPropertyValue[][]> Properties { get; set; }
    }
}
