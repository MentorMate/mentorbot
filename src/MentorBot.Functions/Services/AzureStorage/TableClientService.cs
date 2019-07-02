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
        private readonly IAzureStorageContext _storageContext;

        /// <summary>Initializes a new instance of the <see cref="TableClientService"/> class.</summary>
        public TableClientService(IAzureStorageContext storageContext)
        {
            _storageContext = storageContext;
        }

        /// <inheritdoc/>
        public bool IsConnected => _storageContext != null;

        /// <inheritdoc/>
        public void AddAttributeMapper(IEnumerable<Type> types)
        {
            if (IsConnected)
            {
                foreach (var t in types)
                {
                    _storageContext.AddAttributeMapper(t);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_storageContext != null)
            {
                _storageContext.Dispose();
            }
        }

        /// <inheritdoc/>
        public async Task MergeAsync<T>(IEnumerable<T> models)
            where T : new()
        {
            if (IsConnected)
            {
                await _storageContext.CreateTableAsync<T>();
                await _storageContext.MergeAsync(models);
            }
        }

        /// <inheritdoc/>
        public async Task MergeOrInsertAsync<T>(T model)
            where T : new()
        {
            if (IsConnected)
            {
                await _storageContext.CreateTableAsync<T>();
                await _storageContext.MergeOrInsertAsync(model);
            }
        }

        /// <inheritdoc/>
        public async Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0)
            where T : new()
        {
            if (IsConnected)
            {
                await _storageContext.CreateTableAsync<T>();
                return await _storageContext.QueryAsync<T>(maxItems);
            }

            return Enumerable.Empty<T>().AsQueryable();
        }
    }
}
