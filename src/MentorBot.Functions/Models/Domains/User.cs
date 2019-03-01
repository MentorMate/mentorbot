// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The user domain model.</summary>
    public sealed class User
    {
        /// <summary>Gets or sets the email.</summary>
        public string Email { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the open air user identifier.</summary>
        public long OpenAirUserId { get; set; }

        /// <summary>Gets or sets the department.</summary>
        public Department Department { get; set; }
    }
}
