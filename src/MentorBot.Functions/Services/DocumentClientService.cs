// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace MentorBot.Functions.Services
{
    /// <summary>An document database service for Azure Comtoso DB.</summary>
    public sealed class DocumentClientService : IDocumentClientService
    {
        /// <summary>The connection policy.</summary>
        public static readonly ConnectionPolicy Policy = new ConnectionPolicy
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
        public IDocument<T> Get<T>(string databaseName, string collectionName) =>
            new Document<T>(_client.Value, databaseName, collectionName);

        /// <summary>A document collection resource.</summary>
        /// <typeparam name="T">The document reource type.</typeparam>
        public class Document<T> : IDocument<T>
        {
            private readonly IDocumentClient _client;
            private readonly string _databaseName;
            private readonly string _collectionName;

            /// <summary>Initializes a new instance of the <see cref="Document{T}"/> class.</summary>
            public Document(IDocumentClient client, string databaseName, string collectionName)
            {
                _client = client;
                _databaseName = databaseName;
                _collectionName = collectionName;
            }

            /// <summary>Gets the document collection URI.</summary>
            public Uri DocumentCollectionUri => UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName);

            /// <inheritdoc/>
            public async Task<bool> AddManyAsync(IReadOnlyList<T> items)
            {
                var uri = UriFactory.CreateStoredProcedureUri(_databaseName, _collectionName, "bulkImport");
                var count = await _client.ExecuteStoredProcedureAsync<int>(uri, items);

                return count == items.Count;
            }

            /// <inheritdoc/>
            public async Task<bool> UpdateManyAsync(IReadOnlyList<T> items)
            {
                var uri = UriFactory.CreateStoredProcedureUri(_databaseName, _collectionName, "bulkUpdate");
                var count = await _client.ExecuteStoredProcedureAsync<int>(uri, items);

                return count == items.Count;
            }

            /// <inheritdoc/>
            public Task<bool> AddOrUpdateAsync(T item) =>
                _client.UpsertDocumentAsync(DocumentCollectionUri, item)
                    .ContinueWith(task =>
                        task.Result == null ||
                        task.Result.StatusCode == HttpStatusCode.Created ||
                        task.Result.StatusCode == HttpStatusCode.OK);

            /// <inheritdoc/>
            public IReadOnlyList<T> Query(string sqlExpression) =>
                _client.CreateDocumentQuery<T>(DocumentCollectionUri, sqlExpression).ToList();
        }
    }
}
