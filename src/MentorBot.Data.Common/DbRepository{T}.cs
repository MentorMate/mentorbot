// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Data.Common.Models;

using Microsoft.EntityFrameworkCore;

namespace MentorBot.Data.Common
{
    /// <summary>
    /// The generic entity framework repository.
    /// </summary>
    /// <typeparam name="T">The exact model type.</typeparam>
    /// <seealso cref="MentorBot.Data.Common.IDbRepository{T}" />
    /// <seealso cref="System.IDisposable" />
    public class DbRepository<T> : IDbRepository<T>, IDisposable
        where T : class, IAuditInfo, IDeletableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbRepository{T}"/> class.
        /// </summary>
        public DbRepository(DbContext dbContext)
        {
            var context = dbContext ?? throw new ArgumentException("An instance of DbContext is required to use this repository.", nameof(dbContext));

            this.Context = context;
            this.DbSet = this.Context.Set<T>();
        }

        private DbContext Context { get; }

        private DbSet<T> DbSet { get; }

        /// <inheritdoc/>
        public Task AddEntityAsync(T entity)
        {
            var validatedEntity = entity ?? throw new ArgumentNullException(nameof(entity));

            DbSet.AddAsync(validatedEntity);

            return SaveAsync();
        }

        /// <inheritdoc/>
        public Task<List<T>> AllAsync() => this.DbSet.Where(x => !x.IsDeleted).ToListAsync();

        /// <inheritdoc/>
        public IQueryable<T> AllWithDeleted()
        {
            return this.DbSet;
        }

        /// <inheritdoc/>
        public Task DeleteEntityAsync(T entity) =>
            Task.Run(() =>
            {
                var validatedEntity = entity ?? throw new ArgumentNullException(nameof(entity));

                validatedEntity.IsDeleted = true;
                validatedEntity.DeletedOn = DateTime.UtcNow;
            });

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Context.Dispose();
        }

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync(object id)
        {
            var validId = id ?? throw new ArgumentNullException(nameof(id));

            var item = await this.DbSet.FindAsync(validId).ConfigureAwait(false);

            if (item.IsDeleted)
            {
                return null;
            }

            return item;
        }

        /// <inheritdoc/>
        public Task HardDeleteAsync(T entity) =>
            Task.Run(() =>
            {
                var validatedEntity = entity ?? throw new ArgumentNullException(nameof(entity));

                this.DbSet.Remove(validatedEntity);
            });

        /// <inheritdoc/>
        public Task SaveAsync() => this.Context.SaveChangesAsync();
    }
}
