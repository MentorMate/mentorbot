// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Data.Common.Models
{
    /// <summary>
    /// Provides implementable properties for additional information about creation/updating by user of entity instance.
    /// </summary>
    public interface IEntityWithCreator
    {
        /// <summary>
        /// Gets or sets user that this tuple is created by.
        /// </summary>
        long CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets user that this tuple is last updated by.
        /// </summary>
        long UpdatedBy { get; set; }
    }
}
