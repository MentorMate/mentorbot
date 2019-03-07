// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>A project entity.</summary>
    public sealed class Project
    {
        /// <summary>Gets or sets the identifier.</summary>
        public long OpenAirId { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }
    }
}
