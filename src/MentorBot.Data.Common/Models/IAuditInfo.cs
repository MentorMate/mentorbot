// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

namespace MentorBot.Data.Common.Models
{
    /// <summary>
    /// Provides implementable properties for additional historization information about the entity instance.
    /// </summary>
    public interface IAuditInfo
    {
        /// <summary>
        /// Gets or sets the created on <see cref="DateTime"/>.
        /// </summary>
        DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the modified on <see cref="DateTime"/>.
        /// </summary>
        DateTime? ModifiedOn { get; set; }
    }
}
