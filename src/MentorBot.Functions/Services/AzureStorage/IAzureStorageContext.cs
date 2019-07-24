// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Models;

namespace MentorBot.Functions.Services.AzureStorage
{
    /// <summary>An azure storage client context.</summary>
    public interface IAzureStorageContext : IDisposable
    {
        /// <summary>Adds a database shema mapping.</summary>
        void AddAttributeMapper(Type type);

        /// <summary>Creates a table if not exists asynchronous.</summary>
        Task CreateTableAsync(Type entityType, bool ignoreErrorIfExists = true);

        /// <summary>Queries a table asynchronous.</summary>
        /// <typeparam name="T">The table schema type.</typeparam>
        Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0)
            where T : new();

        /// <summary>Queries a table asynchronous.</summary>
        /// <typeparam name="T">The table schema type.</typeparam>
        Task<IQueryable<T>> QueryAsync<T>(string partitionKey, IEnumerable<QueryFilter> queryFilters, int maxItems = 0)
            where T : new();

        /// <summary>Update or Insert a record in the database asynchronous.</summary>
        /// <typeparam name="T">The table schema type.</typeparam>
        Task MergeOrInsertAsync<T>(T model)
            where T : new();

        /// <summary>Update records in the database asynchronous.</summary>
        /// <typeparam name="T">The table schema type.</typeparam>
        Task MergeAsync<T>(IEnumerable<T> models)
            where T : new();
    }
}
