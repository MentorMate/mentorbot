using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Settings;

namespace MentorBot.Functions.Services
{
    /// <summary>Provides interface to a Azure Table Storage.</summary>
    public sealed class TableStorageService : IStorageService
    {
        private static readonly Regex _disallowedCharsInTableKeys = new Regex(@"[\\\\#%+/?\u0000-\u001F\u007F-\u009F]");
        private static readonly string _disallowedCharReplacement = "-";
        private readonly ITableClientService _tableClientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageService"/> class.
        /// </summary>
        public TableStorageService(ITableClientService tableClientService)
        {
            _tableClientService = tableClientService;

            _tableClientService.AddAttributeMapper(new List<Type> { typeof(MentorBotSettings), typeof(User), typeof(Message), typeof(GoogleAddress) });
        }

        /// <inheritdoc/>
        public async Task<bool> AddUsersAsync(IReadOnlyList<User> users)
        {
            if (!_tableClientService.IsConnected)
            {
                return false;
            }

            foreach (var user in users)
            {
                user.PartitionKey = _disallowedCharsInTableKeys.Replace(user.PartitionKey, _disallowedCharReplacement);
                await _tableClientService.MergeOrInsertAsync<User>(user);
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUsersAsync(IReadOnlyList<User> users)
        {
            if (!_tableClientService.IsConnected)
            {
                return false;
            }

            var groups = users.GroupBy(it => it.PartitionKey);
            foreach (var group in groups)
            {
                await _tableClientService.MergeAsync(group.ToList());
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses)
        {
            if (!_tableClientService.IsConnected)
            {
                return false;
            }

            foreach (var address in addresses)
            {
                address.PartitionKey = _disallowedCharsInTableKeys.Replace(address.SpaceName, _disallowedCharReplacement);
                await _tableClientService.MergeOrInsertAsync<GoogleAddress>(address);
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<GoogleAddress>> GetAddressesAsync()
        {
            var result = await _tableClientService.QueryAsync<GoogleAddress>(2000);

            return result.ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Message>> GetMessagesAsync()
        {
            var result = await _tableClientService.QueryAsync<Message>(2000);

            return result.ToList();
        }

        /// <inheritdoc/>
        public async Task<bool> SaveMessageAsync(Message message)
        {
            if (!_tableClientService.IsConnected)
            {
                return false;
            }

            message.PartitionKey = _disallowedCharsInTableKeys.Replace(message.Input, _disallowedCharReplacement);
            await _tableClientService.MergeOrInsertAsync<Message>(message);

            return true;
        }

        /// <inheritdoc/>
        public async Task<MentorBotSettings> GetSettingsAsync()
        {
            var result = (await _tableClientService.QueryAsync<MentorBotSettings>()).FirstOrDefault();

            if (result == null)
            {
                result = Default.DefaultSettings;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveSettingsAsync(MentorBotSettings settings)
        {
            if (!_tableClientService.IsConnected)
            {
                return false;
            }

            await _tableClientService.MergeOrInsertAsync(settings);

            return true;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<User>> GetAllActiveUsersAsync() =>
            _tableClientService.QueryAsync<User>(2000)
                .ContinueWith(task => (IReadOnlyList<User>)task.Result.Where(user => user.Active).ToList());

        /// <inheritdoc/>
        public Task<IReadOnlyList<User>> GetAllUsersAsync() =>
            _tableClientService.QueryAsync<User>(2000)
                .ContinueWith(task => (IReadOnlyList<User>)task.Result.ToList());
    }
}
