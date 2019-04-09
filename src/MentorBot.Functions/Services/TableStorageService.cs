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
    public sealed class TableStorageService : IStorageService, IDisposable
    {
        private static readonly Regex _disallowedCharsInTableKeys = new Regex(@"[\\\\#%+/?\u0000-\u001F\u007F-\u009F]");
        private static readonly string _disallowedCharReplacement = "-";
        private readonly StorageContext _storageContext;
        private readonly bool _isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageService"/> class.
        /// </summary>
        public TableStorageService(AzureCloudOptions azureCloudOptions)
        {
            _isConnected = azureCloudOptions != null && !string.IsNullOrWhiteSpace(azureCloudOptions.AzureStorageAccountConnectionString);

            if (!_isConnected)
            {
                return;
            }

            try
            {
                _storageContext = new StorageContext(connectionString: azureCloudOptions.AzureStorageAccountConnectionString);

                // ensure we are using the attributes
                _storageContext.AddAttributeMapper(typeof(MentorBotSettings));
                _storageContext.AddAttributeMapper(typeof(User));
                _storageContext.AddAttributeMapper(typeof(Message));
                _storageContext.AddAttributeMapper(typeof(GoogleAddress));
            }
            catch (Exception)
            {
                _isConnected = false;
            }
        }

            /// <inheritdoc/>
        public async Task<bool> AddUsersAsync(IReadOnlyList<User> users)
        {
            if (!_isConnected)
            {
                return false;
            }

            _storageContext.CreateTable<User>();

            foreach (var user in users)
            {
                user.PartitionKey = _disallowedCharsInTableKeys.Replace(user.PartitionKey, _disallowedCharReplacement);
                await _storageContext.MergeOrInsertAsync<User>(user);
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateUsersAsync(IReadOnlyList<User> users)
        {
            if (!_isConnected)
            {
                return false;
            }

            // ensure the table exists
            _storageContext.CreateTable<User>();

            await _storageContext.MergeAsync<User>(users);

            return true;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<User>> GetAllUsersAsync()
        {
            if (!_isConnected)
            {
                return new User[0];
            }

            // ensure the table exists
            _storageContext.CreateTable<User>();

            var result = await _storageContext.QueryAsync<User>(2000);

            return result.ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<User>> GetUsersByIdListAsync(IEnumerable<long> userIdList)
        {
            if (!_isConnected)
            {
                return new User[0];
            }

            // ensure the table exists
            _storageContext.CreateTable<User>();

            var result = await _storageContext.QueryAsync<User>(2000);

            return result.Where(u => userIdList.Contains(u.OpenAirUserId)).ToList();
        }

        /// <inheritdoc/>
        public async Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses)
        {
            if (!_isConnected)
            {
                return false;
            }

            // ensure the table exists
            _storageContext.CreateTable<GoogleAddress>();

            foreach (var address in addresses)
            {
                address.PartitionKey = _disallowedCharsInTableKeys.Replace(address.SpaceName, _disallowedCharReplacement);
                await _storageContext.MergeOrInsertAsync<GoogleAddress>(address);
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<GoogleAddress>> GetAddressesAsync()
        {
            if (!_isConnected)
            {
                return new GoogleAddress[0];
            }

            // ensure the table exists
            _storageContext.CreateTable<GoogleAddress>();

            var result = await _storageContext.QueryAsync<GoogleAddress>(1000);

            return result.ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Message>> GetMessagesAsync()
        {
            if (!_isConnected)
            {
                return new Message[0];
            }

            // ensure the table exists
            _storageContext.CreateTable<Message>();

            var result = await _storageContext.QueryAsync<Message>(2000);

            return result.ToList();
        }

        /// <inheritdoc/>
        public async Task<bool> SaveMessageAsync(Message message)
        {
            if (!_isConnected)
            {
                return false;
            }

            // ensure the table exists
            _storageContext.CreateTable<Message>();

            message.PartitionKey = _disallowedCharsInTableKeys.Replace(message.Input, _disallowedCharReplacement);
            await _storageContext.MergeOrInsertAsync<Message>(message);

            return true;
        }

        /// <inheritdoc/>
        public async Task<MentorBotSettings> GetSettingsAsync()
        {
            if (!_isConnected)
            {
                return new MentorBotSettings();
            }

            // ensure the table exists
            _storageContext.CreateTable<MentorBotSettings>();

            var result = (await _storageContext.QueryAsync<MentorBotSettings>()).FirstOrDefault();

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
            if (!_isConnected)
            {
                return false;
            }

            // ensure the table exists
            _storageContext.CreateTable<MentorBotSettings>();

            await _storageContext.MergeOrInsertAsync<MentorBotSettings>(settings);

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

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_storageContext != null)
            {
                _storageContext.Dispose();
            }
        }
    }
}
