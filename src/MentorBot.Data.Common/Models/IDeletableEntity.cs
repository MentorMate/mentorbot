// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

namespace MentorBot.Data.Common.Models
{
    /// <summary>
    /// Provides implementable properties for additional information about deletion of entity instance.
    /// </summary>
    public interface IDeletableEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the deleted on <see cref="DateTime"/>.
        /// </summary>
        DateTime? DeletedOn { get; set; }
    }
}
