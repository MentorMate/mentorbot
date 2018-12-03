// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace MentorBot.Functions.Services
{
    /// <summary>An document database service for Azure Comtoso DB.</summary>
    public sealed class DocumentClientService : IDocumentClientService
    {
        private static readonly ConnectionPolicy Policy = new ConnectionPolicy
        {
            ConnectionMode = ConnectionMode.Direct,
            ConnectionProtocol = Protocol.Tcp,
            RetryOptions = new RetryOptions
            {
                MaxRetryAttemptsOnThrottledRequests = 10,
                MaxRetryWaitTimeInSeconds = 30
            }
        };

        private readonly string _accountEndpoint;
        private readonly string _authKeyOrResourceToken;
        private readonly Lazy<IDocumentClient> _client;

        /// <summary>Initializes a new instance of the <see cref="DocumentClientService"/> class.</summary>
        public DocumentClientService(string accountEndpoint, string authKeyOrResourceToken)
        {
            _accountEndpoint = accountEndpoint;
            _authKeyOrResourceToken = authKeyOrResourceToken;
            _client = new Lazy<IDocumentClient>(() => new DocumentClient(new Uri(_accountEndpoint), _authKeyOrResourceToken, Policy));
        }

        /// <inheritdoc/>
        public bool IsConnected => !string.IsNullOrEmpty(_accountEndpoint) && !string.IsNullOrEmpty(_authKeyOrResourceToken);

        /// <inheritdoc/>
        public async Task<IDocument<T>> GetAsync<T>(string databaseName, string collectionName)
        {
            var client = _client.Value;
            var db = new Database { Id = databaseName };
            var documentCollection = new DocumentCollection { Id = collectionName };

            var databaseLink = UriFactory.CreateDatabaseUri(databaseName);
            var documentCollectionLink = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);

            await client.CreateDatabaseIfNotExistsAsync(db)
                        .ConfigureAwait(false);

            await client.CreateDocumentCollectionIfNotExistsAsync(databaseLink, documentCollection)
                        .ConfigureAwait(false);

            return new Document<T>(client, documentCollectionLink);
        }

        /// <summary>A document collection resource.</summary>
        /// <typeparam name="T">The document reource type.</typeparam>
        public class Document<T> : IDocument<T>
        {
            private readonly IDocumentClient _client;
            private readonly Uri _documentCollectionUri;

            /// <summary>Initializes a new instance of the <see cref="Document{T}"/> class.</summary>
            public Document(IDocumentClient client, Uri documentCollectionUri)
            {
                _client = client;
                _documentCollectionUri = documentCollectionUri;
            }

            /// <inheritdoc/>
            public Task AddAsync(T item) =>
                _client.CreateDocumentAsync(_documentCollectionUri, item);

            /// <inheritdoc/>
            public IReadOnlyList<T> Query(string sqlExpression) =>
                _client.CreateDocumentQuery<T>(_documentCollectionUri, sqlExpression)
                       .ToList();
        }
    }
}
