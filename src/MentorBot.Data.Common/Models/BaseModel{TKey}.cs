// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace MentorBot.Data.Common.Models
{
    /// <summary>
    /// The base model for all of the entities. Contains additional historization information.
    /// </summary>
    /// <typeparam name="TKey">The type of the identifier key.</typeparam>
    /// <seealso cref="MentorBot.Data.Common.Models.IAuditInfo" />
    /// <seealso cref="MentorBot.Data.Common.Models.IDeletableEntity" />
    public abstract class BaseModel<TKey> : IAuditInfo, IDeletableEntity
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [Key]
        public TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the created on <see cref="DateTime"/>.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the modified on <see cref="DateTime"/>.
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the deleted on <see cref="DateTime"/>.
        /// </summary>
        public DateTime? DeletedOn { get; set; }
    }
}
