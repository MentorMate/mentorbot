// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Settings;

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
             QueryWhenConnected<GoogleAddress>(
                 "SELECT TOP 1000 * FROM addresses",
                 AddressDocumentName);

        /// <inheritdoc/>
        public IReadOnlyList<Message> GetMessages() =>
             QueryWhenConnected<Message>(
                 "SELECT TOP 1000 m.ProbabilityPercentage FROM messages m",
                 MessagesDocumentName);

        /// <inheritdoc/>
        public Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses) =>
            ExecuteIfConnectedAsync<GoogleAddress, bool>(doc => doc.AddManyAsync(addresses), AddressDocumentName, false);

        /// <inheritdoc/>
        public IReadOnlyList<User> GetAllUsers() =>
            QueryWhenConnected<User>("SELECT TOP 2000 * FROM users", UserDocumentName);

        /// <inheritdoc/>
        public IReadOnlyList<User> GetUsersByIdList(IEnumerable<long> userIdList) =>
            userIdList != null && userIdList.Any() ?
                QueryWhenConnected<User>($"SELECT TOP 1000 * FROM users u WHERE u.OpenAirUserId in ({string.Join(',', userIdList)})", UserDocumentName) :
                new User[0];

        /// <inheritdoc/>
        public Task<bool> AddUsersAsync(IReadOnlyList<User> users) =>
            ExecuteIfConnectedAsync<User, bool>(doc => doc.AddManyAsync(users), UserDocumentName, false);

        /// <inheritdoc/>
        public Task<bool> UpdateUsersAsync(IReadOnlyList<User> users) =>
            ExecuteIfConnectedAsync<User, bool>(doc => doc.UpdateManyAsync(users), UserDocumentName, false);

        /// <inheritdoc/>
        public Task<MentorBotSettings> GetSettingsAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> SaveSettingsAsync(MentorBotSettings settings)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<GoogleAddress>> GetAddressesAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<User>> GetUsersByIdListAsync(IEnumerable<long> userIdList)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<User>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<Message>> GetMessagesAsync()
        {
            throw new NotImplementedException();
        }

        private IReadOnlyList<T> QueryWhenConnected<T>(string sqlException, string documentName)
        {
            if (_documentClientService.IsConnected)
            {
                return _documentClientService.Get<T>(DatabaseName, documentName).Query(sqlException);
            }

            return new T[0];
        }

        private async Task<T> ExecuteIfConnectedAsync<F, T>(Func<IDocument<F>, Task<T>> action, string documentName, T defaultValue)
        {
            if (_documentClientService.IsConnected)
            {
                var document = _documentClientService.Get<F>(DatabaseName, documentName);
                return await action(document);
            }

            return defaultValue;
        }
    }
}
