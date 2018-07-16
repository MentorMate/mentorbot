// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using MentorBot.Data.Common.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Win32.SafeHandles;

namespace MentorBot.Data.Common
{
    /// <summary>
    /// The generic entity framework repository.
    /// </summary>
    /// <typeparam name="T">The exact model type.</typeparam>
    /// <seealso cref="MentorBot.Data.Common.IDbRepository{T}" />
    /// <seealso cref="System.IDisposable" />
    public abstract class DbRepository<T> : IDisposable
        where T : class, IAuditInfo, IDeletableEntity
    {
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly SafeHandle _handle;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbRepository{T}"/> class.
        /// </summary>
        protected DbRepository(DbContext dbContext)
        {
            var context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            _handle = new SafeFileHandle(IntPtr.Zero, true);
            _context = context;
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Adds the changes to the given entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <exception cref="ArgumentNullException">The given entity should have value. - entity</exception>
        public Task AddEntityAsync(T entity)
        {
            var validatedEntity = entity ?? throw new ArgumentNullException(nameof(entity));

            _dbSet.AddAsync(validatedEntity);

            return SaveAsync();
        }

        /// <summary>
        /// Gets all the records without deleted ones asynchronously.
        /// </summary>
        public Task<List<T>> AllAsync() => _dbSet.Where(x => !x.IsDeleted).ToListAsync();

        /// <summary>
        /// Gets all the records with the deleted.
        /// </summary>
        public IQueryable<T> AllWithDeleted() => _dbSet;

        /// <summary>
        /// Sets as deleted the given entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to delete from.</param>
        public Task DeleteEntityAsync(T entity)
        {
            var validatedEntity = entity ?? throw new ArgumentNullException(nameof(entity));

            validatedEntity.IsDeleted = true;
            validatedEntity.DeletedOn = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates the given entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public Task UpdateAsync(T entity)
        {
            var validatedEntity = entity ?? throw new ArgumentNullException(nameof(entity));

            _dbSet.Update(validatedEntity);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the certain record by identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier to search by.</param>
        /// <exception cref="ArgumentException">The given id should have value. - id</exception>
        public async Task<T> GetByIdAsync(object id)
        {
            var validId = id ?? throw new ArgumentNullException(nameof(id));

            var item = await _dbSet.FindAsync(validId);

            if (item.IsDeleted)
            {
                return null;
            }

            return item;
        }

        /// <summary>
        /// Hards deletes the given entity asynchronously.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public Task HardDeleteAsync(T entity)
        {
            var validatedEntity = entity ?? throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(validatedEntity);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves asynchronously applied changes.
        /// </summary>
        public Task SaveAsync() => _context.SaveChangesAsync();

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">Whether to dispose the object.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                _handle.Dispose();

                _context.Dispose();
            }

            disposed = true;
        }
    }
}
