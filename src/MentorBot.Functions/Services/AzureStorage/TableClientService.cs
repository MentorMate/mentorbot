// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;

namespace MentorBot.Functions.Services.AzureStorage
{
    /// <summary>Handle access to a Table storage.</summary>
    public sealed class TableClientService : ITableClientService, IDisposable
    {
        private readonly List<string> _mappedTypes = new List<string>();

        private readonly IAzureStorageContext _storageContext;

        /// <summary>Initializes a new instance of the <see cref="TableClientService"/> class.</summary>
        public TableClientService(IAzureStorageContext storageContext)
        {
            _storageContext = storageContext;
        }

        /// <inheritdoc/>
        public bool IsConnected => _storageContext != null;

        /// <inheritdoc/>
        public Task MergeAsync<T>(IEnumerable<T> models)
            where T : new() =>
            ExecuteAsync<T>(ctx => ctx.MergeAsync(models));

        /// <inheritdoc/>
        public Task MergeOrInsertAsync<T>(T model)
            where T : new() =>
            ExecuteAsync<T>(ctx => ctx.MergeOrInsertAsync(model));

        /// <inheritdoc/>
        public Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0)
            where T : new() =>
            QueryAsync(ctx => ctx.QueryAsync<T>(maxItems));

        /// <inheritdoc/>
        public Task<IQueryable<T>> QueryAsync<T>(string query, int maxItems = 0)
            where T : new() =>
            QueryAsync(ctx => ctx.QueryAsync<T>(null, ctx.CreateQueryFilters(query), maxItems));

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_storageContext != null)
            {
                _storageContext.Dispose();
            }
        }

        private Task ExecuteAsync<T>(Func<IAzureStorageContext, Task> action)
        {
            if (IsConnected)
            {
                AddMapper(typeof(T));
                return action(_storageContext);
            }

            return Task.CompletedTask;
        }

        private async Task<IQueryable<T>> QueryAsync<T>(Func<IAzureStorageContext, Task<IQueryable<T>>> action)
        {
            if (IsConnected)
            {
                AddMapper(typeof(T));
                return await action(_storageContext);
            }

            return Enumerable.Empty<T>().AsQueryable();
        }

        private void AddMapper(Type type)
        {
            if (!_mappedTypes.Contains(type.FullName))
            {
                _storageContext.AddAttributeMapper(type);
                _storageContext.CreateTableAsync(type, true);
                _mappedTypes.Add(type.FullName);
            }
        }
    }
}
