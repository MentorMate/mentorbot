// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;

namespace MentorBot.Functions.Services
{
    /// <summary>An store service that provide data from the document client.</summary>
    public sealed class StorageService : IStorageService
    {
        private const string DatabaseName = "mentorbot";
        private const string UserDocumentName = "users";
        private const string AddressDocumentName = "addresses";
        private const string MessagesDocumentName = "messages";

        private readonly IDocumentClientService _documentClientService;

        /// <summary>Initializes a new instance of the <see cref="StorageService"/> class.</summary>
        public StorageService(IDocumentClientService documentClientService)
        {
            _documentClientService = documentClientService;
        }

        /// <inheritdoc/>
        public IReadOnlyList<GoogleAddress> GetAddresses() =>
             QueryWhenConnectedAsync<GoogleAddress>(
                 "SELECT TOP 1000 * FROM addresses",
                 AddressDocumentName);

        /// <inheritdoc/>
        public IReadOnlyList<Message> GetMessages() =>
             QueryWhenConnectedAsync<Message>(
                 "SELECT TOP 1000 m.ProbabilityPercentage FROM messages m",
                 MessagesDocumentName);

        /// <inheritdoc/>
        public Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses) =>
            AddAsync(addresses, AddressDocumentName);

        /// <inheritdoc/>
        public User GetUserByEmail(string email) =>
            QueryWhenConnectedAsync<User>($"SELECT TOP 1 * FROM users u WHERE u.Email = \"{email}\"", UserDocumentName)
                .FirstOrDefault();

        /// <inheritdoc/>
        public IReadOnlyList<User> GetUsersByIdList(IEnumerable<long> userIdList) =>
            userIdList != null && userIdList.Any() ?
                QueryWhenConnectedAsync<User>($"SELECT TOP 1000 * FROM users u WHERE u.OpenAirUserId in ({string.Join(',', userIdList)})", UserDocumentName) :
                new User[0];

        /// <inheritdoc/>
        public Task<bool> AddUsersAsync(IReadOnlyList<User> users) =>
            AddAsync(users, UserDocumentName);

        private IReadOnlyList<T> QueryWhenConnectedAsync<T>(string sqlException, string documentName)
            where T : class
        {
            if (_documentClientService.IsConnected)
            {
                return _documentClientService.Get<T>(DatabaseName, documentName).Query(sqlException);
            }

            return new T[0];
        }

        private async Task<bool> AddAsync<T>(IReadOnlyList<T> domainModels, string documentName)
        {
            if (_documentClientService.IsConnected)
            {
                return await _documentClientService.Get<T>(DatabaseName, documentName).AddManyAsync(domainModels);
            }

            return false;
        }
    }
}
