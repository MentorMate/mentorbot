// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Models.Domains;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>The service that store domain models.</summary>
    public interface IStorageService
    {
        /// <summary>Gets the addresses.</summary>
        IReadOnlyList<GoogleAddress> GetAddresses();

        /// <summary>Adds the addresses asynchronous.</summary>
        Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses);

        /// <summary>Gets the user by list of identifiers.</summary>
        /// <param name="userIdList">The list of user identifiers.</param>
        IReadOnlyList<User> GetUsersByIdList(IEnumerable<long> userIdList);

        /// <summary>Gets all stored users.</summary>
        IReadOnlyList<User> GetAllUsers();

        /// <summary>Adds the users asynchronous.</summary>
        Task<bool> AddUsersAsync(IReadOnlyList<User> users);

        /// <summary>Update the users asynchronous.</summary>
        Task<bool> UpdateUsersAsync(IReadOnlyList<User> users);

        /// <summary>Gets the messages.</summary>
        IReadOnlyList<Message> GetMessages();
    }
}
