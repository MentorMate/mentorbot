using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>Handle access to a Table storage.</summary>
    public interface ITableClientService
    {
        /// <summary>Gets a value indicating whether this instance is connected.</summary>
        bool IsConnected { get; }

        /// <summary>
        /// Adds the specified types to the Attribute mapper.
        /// </summary>
        /// <param name="types">A list of types to register.</param>
        void AddAttributeMapper(IEnumerable<Type> types);

        /// <summary>
        /// Creates a new table for the specified type if such table does not exists.
        /// </summary>
        /// <typeparam name="T">The tyme of the model for the created table.</typeparam>
        /// <param name="ignoreErrorIfExists">If true the erroe will be ignored if the table already exists.</param>
        void CreateTable<T>(bool ignoreErrorIfExists = true);

        /// <summary>
        /// Inserts new or updates existing record.
        /// </summary>
        /// <typeparam name="T">The model type for the table.</typeparam>
        /// <param name="model">The model instance that should be inserted or updated.</param>
        /// <returns>Task</returns>
        Task MergeOrInsertAsync<T>(T model)
            where T : new();

        /// <summary>
        /// Updates existing records.
        /// </summary>
        /// <typeparam name="T">The model type for the table.</typeparam>
        /// <param name="models">A list of model instances to be updated.</param>
        /// <returns>Task</returns>
        Task MergeAsync<T>(IEnumerable<T> models)
            where T : new();

        /// <summary>
        /// Executes a query against table of type T and limits the result to maxItems.
        /// </summary>
        /// <typeparam name="T">Table model type</typeparam>
        /// <param name="maxItems">The max number of items to be returned.</param>
        /// <returns>IQueryable from the specified type.</returns>
        Task<IQueryable<T>> QueryAsync<T>(int maxItems = 0)
            where T : new();
    }
}
