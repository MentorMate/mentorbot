// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Models.Domains;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>The service that store domain models.</summary>
    public interface IStorageService
    {
        /// <summary>Gets the addresses asynchronous.</summary>
        Task<IReadOnlyList<GoogleAddress>> GetAddressesAsync();

        /// <summary>Adds the address asynchronous.</summary>
        Task AddAddressAsync(GoogleAddress address);

        /// <summary>Gets the user by email.</summary>
        /// <param name="email">The email.</param>
        Task<User> GetUserByEmailAsync(string email);

        /// <summary>Gets the user by list of identifiers.</summary>
        /// <param name="userIdList">The list of user identifiers.</param>
        Task<IReadOnlyList<User>> GetUsersByIdListAsync(IEnumerable<long> userIdList);

        /// <summary>Adds the user asynchronous.</summary>
        Task AddUserAsync(User user);
    }
}
