// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>Handle access to a document database.</summary>
    public interface IDocumentClientService
    {
        /// <summary>Gets a value indicating whether this instance is connected.</summary>
        bool IsConnected { get; }

        /// <summary>Get document database querable object.</summary>
        /// <typeparam name="T">The type of the document collection object.</typeparam>
        /// <param name="databaseName">The database name.</param>
        /// <param name="collectionName">The collection name.</param>
        Task<IDocument<T>> GetAsync<T>(string databaseName, string collectionName);
    }
}
