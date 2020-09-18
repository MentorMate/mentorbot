// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;

namespace MentorBot.Functions.Services
{
    /// <summary>An store service that provide data from the document client.</summary>
    public sealed class StorageService : IStorageService
    {
        private const string DatabaseName = "mentorbot";
        private const string UserDocumentName = "users";
        private const string StatisticsDocumentName = "statistics";
        private const string AddressDocumentName = "addresses";
        private const string MessagesDocumentName = "messages";
        private const string PluginsDocumentName = "plugins";

        private readonly IDocumentClientService _documentClientService;

        /// <summary>Initializes a new instance of the <see cref="StorageService"/> class.</summary>
        public StorageService(IDocumentClientService documentClientService)
        {
            _documentClientService = documentClientService;
        }

        /// <inheritdoc/>
        public Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses) =>
            ExecuteIfConnectedAsync<GoogleAddress, bool>(doc => doc.AddManyAsync(addresses), AddressDocumentName, false);

        /// <inheritdoc/>
        public Task<IReadOnlyList<User>> GetAllUsersAsync() =>
            Task.FromResult(
                QueryWhenConnected<User>("SELECT TOP 2000 * FROM users", UserDocumentName));

        /// <inheritdoc/>
        public Task<IReadOnlyList<User>> GetAllActiveUsersAsync() =>
            Task.FromResult(
                QueryWhenConnected<User>("SELECT TOP 1000 * FROM users u WHERE u.Active == 1", UserDocumentName));

        /// <inheritdoc/>
        public Task<User> GetUserByEmailAsync(string email) =>
            Task.FromResult(
                QueryWhenConnected<User>($"SELECT TOP 1 * FROM users u WHERE u.Email == '{email}'", UserDocumentName).FirstOrDefault());

        /// <inheritdoc/>
        public Task<User> GetUserByIdAsync(string userId) =>
            Task.FromResult(
                QueryWhenConnected<User>($"SELECT TOP 1 * FROM users u WHERE u.id == '{userId}'", UserDocumentName).FirstOrDefault());

        /// <inheritdoc/>
        public Task<IReadOnlyList<Plugin>> GetAllPluginsAsync() =>
            Task.FromResult(
                QueryWhenConnected<Plugin>("SELECT TOP 2000 * FROM " + PluginsDocumentName, PluginsDocumentName));

        /// <inheritdoc/>
        public Task<bool> AddUsersAsync(IReadOnlyList<User> users) =>
            ExecuteIfConnectedAsync<User, bool>(doc => doc.AddManyAsync(users), UserDocumentName, false);

        /// <inheritdoc/>
        public Task<bool> UpdateUsersAsync(IReadOnlyList<User> users) =>
            ExecuteIfConnectedAsync<User, bool>(doc => doc.UpdateManyAsync(users), UserDocumentName, false);

        /// <inheritdoc/>
        public Task<bool> AddOrUpdatePluginsAsync(IReadOnlyList<Plugin> plugins) =>
            ExecuteIfConnectedAsync<Plugin, bool>(
                async doc =>
                    await doc.AddManyAsync(plugins.Where(it => it.Id == null).ToList()) ||
                    await doc.UpdateManyAsync(plugins.Where(it => it.Id != null).ToList()),
                PluginsDocumentName,
                false);

        /// <inheritdoc/>
        public Task<IReadOnlyList<GoogleAddress>> GetAddressesAsync() =>
            Task.FromResult(
                QueryWhenConnected<GoogleAddress>(
                    "SELECT TOP 1000 * FROM addresses",
                    AddressDocumentName));

        /// <inheritdoc/>
        public Task<IReadOnlyList<Message>> GetMessagesAsync() =>
            Task.FromResult(
                QueryWhenConnected<Message>(
                    "SELECT TOP 2000 m.ProbabilityPercentage FROM messages m",
                    MessagesDocumentName));

        /// <inheritdoc/>
        public Task<bool> SaveMessageAsync(Message message) =>
            ExecuteIfConnectedAsync<Message, bool>(doc => doc.AddOrUpdateAsync(message), MessagesDocumentName, false);

        /// <inheritdoc/>
        public Task<bool> AddOrUpdateStatisticsAsync<T>(Statistics<T> data) =>
            ExecuteIfConnectedAsync<Statistics<T>, bool>(doc => doc.AddOrUpdateAsync(data), StatisticsDocumentName, false);

        /// <inheritdoc/>
        public Task<bool> AddOrUpdateUserAsync(User user) =>
            ExecuteIfConnectedAsync<User, bool>(doc => doc.AddOrUpdateAsync(user), UserDocumentName, false);

        /// <inheritdoc/>
        public Task<IReadOnlyList<Statistics<T>>> GetStatisticsAsync<T>(string date, string time) =>
            Task.FromResult(QueryWhenConnected<Statistics<T>>(
                $"SELECT TOP 1000 * FROM {StatisticsDocumentName} s WHERE s.Data == '{date}' AND s.Time == '{time}'", StatisticsDocumentName));

        private IReadOnlyList<T> QueryWhenConnected<T>(string sqlException, string documentName)
        {
            if (_documentClientService.IsConnected)
            {
                return _documentClientService.Get<T>(DatabaseName, documentName).Query(sqlException);
            }

            return new T[0];
        }

        private async Task<T> ExecuteIfConnectedAsync<TModel, T>(Func<IDocument<TModel>, Task<T>> action, string documentName, T defaultValue)
        {
            if (_documentClientService.IsConnected)
            {
                var document = _documentClientService.Get<TModel>(DatabaseName, documentName);
                return await action?.Invoke(document);
            }

            return defaultValue;
        }
    }
}
