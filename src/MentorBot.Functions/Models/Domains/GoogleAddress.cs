// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The google adress metadata.</summary>
    public sealed class GoogleAddress
    {
        /// <summary>Gets or sets the name of the space.</summary>
        public string SpaceName { get; set; }

        /// <summary>Gets or sets the name of the user.</summary>
        public string UserName { get; set; }

        /// <summary>Gets or sets the email of the user.</summary>
        public string UserEmail { get; set; }

        /// <summary>Gets or sets the display name of the user.</summary>
        public string UserDisplayName { get; set; }
    }
}
