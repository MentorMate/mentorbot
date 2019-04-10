using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Options;
using MentorBot.Functions.Models.Settings;

namespace MentorBot.Functions.Services
{
    /// <summary>
    ///  Provides interface to a Azure Table Storage
    /// </summary>
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

            await _tableClientService.MergeAsync<User>(users);

            return true;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<User>> GetAllUsersAsync()
        {
            var result = await _tableClientService.QueryAsync<User>(2000);

            return result.ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<User>> GetUsersByIdListAsync(IEnumerable<long> userIdList)
        {
            var result = await _tableClientService.QueryAsync<User>(2000);

            return result.Where(u => userIdList.Contains(u.OpenAirUserId)).ToList();
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
            var result = await _tableClientService.QueryAsync<GoogleAddress>(1000);

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
                result = new MentorBotSettings();
                ServiceLocator.EnsureServiceProvider();
                var processors = ServiceLocator.GetServices<ICommandProcessor>();

                result.Processors = processors.Select(p => new ProcessorSettings { Name = p.GetType().Name, Enabled = true }).ToList();
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

            await _tableClientService.MergeOrInsertAsync<MentorBotSettings>(settings);

            return true;
        }

        /// <inheritdoc/>
        public IReadOnlyList<GoogleAddress> GetAddresses()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IReadOnlyList<User> GetUsersByIdList(IEnumerable<long> userIdList)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IReadOnlyList<User> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Message> GetMessages()
        {
            throw new NotImplementedException();
        }
    }
}
