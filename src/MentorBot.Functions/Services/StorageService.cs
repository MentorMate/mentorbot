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

        private readonly IDocumentClientService _documentClientService;

        /// <summary>Initializes a new instance of the <see cref="StorageService"/> class.</summary>
        public StorageService(IDocumentClientService documentClientService)
        {
            _documentClientService = documentClientService;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<GoogleAddress>> GetAddressesAsync() =>
             QueryWhenConnectedAsync<GoogleAddress>(
                 "SELECT TOP 1000 * FROM addresses",
                 AddressDocumentName);

        /// <inheritdoc/>
        public Task AddAddressAsync(GoogleAddress address) =>
            AddAsync(address, AddressDocumentName);

        /// <inheritdoc/>
        public Task<User> GetUserByEmailAsync(string email) =>
            QueryWhenConnectedAsync<User>($"SELECT TOP 1 * FROM users u WHERE u.Email = \"{email}\"", UserDocumentName)
                .ContinueWith(task => task.Result.FirstOrDefault());

        /// <inheritdoc/>
        public Task<IReadOnlyList<User>> GetUsersByIdListAsync(IEnumerable<long> userIdList) =>
            userIdList != null && userIdList.Any() ?
                QueryWhenConnectedAsync<User>($"SELECT TOP 1000 * FROM users u WHERE u.OpenAirUserId in ({string.Join(',', userIdList)})", UserDocumentName) :
                Task.FromResult<IReadOnlyList<User>>(new User[0]);

        /// <inheritdoc/>
        public Task AddUserAsync(User user) =>
            AddAsync(user, UserDocumentName);

        private async Task<IReadOnlyList<T>> QueryWhenConnectedAsync<T>(string sqlException, string documentName)
            where T : class
        {
            if (_documentClientService.IsConnected)
            {
                var document = await _documentClientService.GetAsync<T>(DatabaseName, documentName);
                return document.Query(sqlException);
            }

            return new T[0];
        }

        private async Task AddAsync<T>(T domainModel, string documentName)
        {
            if (_documentClientService.IsConnected)
            {
                var document = await _documentClientService.GetAsync<T>(DatabaseName, documentName);
                await document.AddAsync(domainModel);
            }
        }
    }
}
