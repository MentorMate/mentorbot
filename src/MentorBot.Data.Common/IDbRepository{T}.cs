// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Data.Common.Models;

namespace MentorBot.Data.Common
{
    /// <summary>
    /// The generic entity framework repository interface.
    /// </summary>
    /// <typeparam name="T">The exact model type.</typeparam>
    public interface IDbRepository<T>
        where T : class, IAuditInfo, IDeletableEntity
    {
        /// <summary>
        /// Gets all the records without deleted ones asynchronously.
        /// </summary>
        Task<List<T>> AllAsync();

        /// <summary>
        /// Gets all the records with the deleted.
        /// </summary>
        IQueryable<T> AllWithDeleted();

        /// <summary>
        /// Gets the certain record by identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier to search by.</param>
        /// <exception cref="ArgumentException">The given id should have value. - id</exception>
        Task<T> GetByIdAsync(object id);

        /// <summary>
        /// Adds the changes to the given entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <exception cref="ArgumentNullException">The given entity should have value. - entity</exception>
        Task AddEntityAsync(T entity);

        /// <summary>
        /// Sets as deleted the given entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to delete from.</param>
        Task DeleteEntityAsync(T entity);

        /// <summary>
        /// Hards deletes the given entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity.</param>
        Task HardDeleteAsync(T entity);

        /// <summary>
        /// Saves asynchronously applied changes.
        /// </summary>
        Task SaveAsync();
    }
}
