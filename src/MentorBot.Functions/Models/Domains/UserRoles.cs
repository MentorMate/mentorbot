using System;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The user roles.</summary>
    [Flags]
    public enum UserRoles
    {
        /// <summary>Anonymous user.</summary>
        None = 0,

        /// <summary>An ordinary user that can only log in.</summary>
        User = 1,

        /// <summary>A system administrator user.</summary>
        Administrator = 2
    }
}
