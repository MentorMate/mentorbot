// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Settings;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>The service that store domain models.</summary>
    public interface IStorageService
    {
        /// <summary>Gets the addresses asynchronous.</summary>
        Task<IReadOnlyList<GoogleAddress>> GetAddressesAsync();

        /// <summary>Gets the addresses.</summary>
        IReadOnlyList<GoogleAddress> GetAddresses();

        /// <summary>Adds the addresses asynchronous.</summary>
        Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses);

        /// <summary>Gets the user by list of identifiers.</summary>
        /// <param name="userIdList">The list of user identifiers.</param>
        IReadOnlyList<User> GetUsersByIdList(IEnumerable<long> userIdList);

        /// <summary>Gets the user by list of identifiers asynchronous.</summary>
        /// <param name="userIdList">The list of user identifiers.</param>
        Task<IReadOnlyList<User>> GetUsersByIdListAsync(IEnumerable<long> userIdList);

        /// <summary>Gets all stored users.</summary>
        IReadOnlyList<User> GetAllUsers();

        /// <summary>Gets all stored users asynchronous.</summary>
        Task<IReadOnlyList<User>> GetAllUsersAsync();

        /// <summary>Adds the users asynchronous.</summary>
        Task<bool> AddUsersAsync(IReadOnlyList<User> users);

        /// <summary>Update the users asynchronous.</summary>
        Task<bool> UpdateUsersAsync(IReadOnlyList<User> users);

        /// <summary>Gets the messages asynchronous.</summary>
        Task<IReadOnlyList<Message>> GetMessagesAsync();

        /// <summary>Gets the messages.</summary>
        IReadOnlyList<Message> GetMessages();

        /// <summary>Saves a message.</summary>
        Task<bool> SaveMessageAsync(Message message);

        /// <summary>
        /// Reads the settings from the Table Storage
        /// </summary>
        /// <returns>An instance of the <see cref="MentorBotSettings"/> class</returns>
        Task<MentorBotSettings> GetSettingsAsync();

        /// <summary>
        /// Saves the passed settings to the Table Storage
        /// </summary>
        /// <param name="settings">An object that contains the latest settings.</param>
        /// <returns>True if the save is successful</returns>
        Task<bool> SaveSettingsAsync(MentorBotSettings settings);
    }
}
