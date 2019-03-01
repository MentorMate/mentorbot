// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;

using Microsoft.Azure.CosmosDB.BulkExecutor;
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
            : this(new Lazy<IDocumentClient>(() => new DocumentClient(new Uri(accountEndpoint), authKeyOrResourceToken, Policy)))
        {
            _accountEndpoint = accountEndpoint;
            _authKeyOrResourceToken = authKeyOrResourceToken;
        }

        /// <summary>Initializes a new instance of the <see cref="DocumentClientService"/> class.</summary>
        public DocumentClientService(Lazy<IDocumentClient> documentClient)
        {
            _client = documentClient;
        }

        /// <inheritdoc/>
        public bool IsConnected => !string.IsNullOrEmpty(_accountEndpoint) && !string.IsNullOrEmpty(_authKeyOrResourceToken);

        /// <inheritdoc/>
        public IDocument<T> Get<T>(string databaseName, string collectionName)
        {
            var client = _client.Value;
            var documentCollection = new DocumentCollection { Id = collectionName };

            var documentCollectionLink = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);

            return new Document<T>(client, documentCollectionLink, documentCollection);
        }

        /// <summary>A document collection resource.</summary>
        /// <typeparam name="T">The document reource type.</typeparam>
        public class Document<T> : IDocument<T>
        {
            private readonly IDocumentClient _client;
            private readonly Uri _documentCollectionUri;
            private readonly DocumentCollection _documentCollection;

            /// <summary>Initializes a new instance of the <see cref="Document{T}"/> class.</summary>
            public Document(IDocumentClient client, Uri documentCollectionUri, DocumentCollection documentCollection)
            {
                _client = client;
                _documentCollectionUri = documentCollectionUri;
                _documentCollection = documentCollection;
            }

            /// <inheritdoc/>
            public async Task<bool> AddManyAsync(IReadOnlyList<T> items)
            {
                if (_client is DocumentClient documentClient)
                {
                    var executor = new BulkExecutor(documentClient, _documentCollection);

                    await executor.InitializeAsync();

                    var response = await executor.BulkImportAsync(items.Cast<object>());

                    return response.NumberOfDocumentsImported == items.Count;
                }

                return false;
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
