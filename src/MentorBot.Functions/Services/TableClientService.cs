using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Options;

namespace MentorBot.Functions.Services
{
    /// <summary>Handle access to a Table storage.</summary>
    public class TableClientService : ITableClientService, IDisposable
    {
        private readonly StorageContext _storageContext;
        private bool disposed = false;

        /// <summary>Initializes a new instance of the <see cref="TableClientService"/> class.</summary>
        public TableClientService(AzureCloudOptions azureCloudOptions)
        {
            try
            {
                _storageContext = new StorageContext(connectionString: azureCloudOptions.AzureStorageAccountConnectionString);
            }
            catch (Exception)
            {
                _storageContext = null;
            }
        }

        /// <inheritdoc/>
        public bool IsConnected => _storageContext != null;

        /// <inheritdoc/>
        public void AddAttributeMapper(IEnumerable<Type> types)
        {
            if (!IsConnected)
            {
                return;
            }

            foreach (var t in types)
            {
                _storageContext.AddAttributeMapper(t);
            }
        }

        /// <inheritdoc/>
        public void CreateTable<T>(bool ignoreErrorIfExists = true)
        {
            if (!IsConnected)
            {
                return;
            }

            _storageContext.CreateTable<T>(ignoreErrorIfExists);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async Task MergeAsync<T>(IEnumerable<T> models)
            where T : new()
        {
            if (!IsConnected)
            {
                return;
            }

            _storageContext.CreateTable<T>();

            await _storageContext.MergeAsync<T>(models);
        }

        /// <inheritdoc/>
        public async Task MergeOrInsertAsync<T>(T model)
            where T : new()
        {
            if (!IsConnected)
            {
                return;
            }

            _storageContext.CreateTable<T>();

            await _storageContext.MergeOrInsertAsync<T>(model);
        }

        /// <inheritdoc/>
        public async Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0)
            where T : new()
        {
            if (!IsConnected)
            {
                return Enumerable.Empty<T>().AsQueryable();
            }

            _storageContext.CreateTable<T>();

            return await _storageContext.QueryAsync<T>(maxItems);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                _storageContext.Dispose();
            }

            disposed = true;
        }
    }
}
