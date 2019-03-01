// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>A company department.</summary>
    public sealed class Department
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the open air department identifier.</summary>
        public long? OpenAirDepartmentId { get; set; }
    }
}
