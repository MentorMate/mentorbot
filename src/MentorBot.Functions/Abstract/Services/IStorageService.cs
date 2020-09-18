// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>The service that store domain models.</summary>
    public interface IStorageService
    {
        /// <summary>Gets the addresses asynchronous.</summary>
        Task<IReadOnlyList<GoogleAddress>> GetAddressesAsync();

        /// <summary>Adds the addresses asynchronous.</summary>
        Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses);

        /// <summary>Gets all stored active users.</summary>
        Task<IReadOnlyList<User>> GetAllActiveUsersAsync();

        /// <summary>Gets all stored users.</summary>
        Task<IReadOnlyList<User>> GetAllUsersAsync();

        /// <summary>Gets stored user by email.</summary>
        Task<User> GetUserByEmailAsync(string email);

        /// <summary>Gets stored user by id.</summary>
        Task<User> GetUserByIdAsync(string userId);

        /// <summary>Gets all plugins asynchronous.</summary>
        Task<IReadOnlyList<Plugin>> GetAllPluginsAsync();

        /// <summary>Adds the users asynchronous.</summary>
        Task<bool> AddUsersAsync(IReadOnlyList<User> users);

        /// <summary>Update the users asynchronous.</summary>
        Task<bool> UpdateUsersAsync(IReadOnlyList<User> users);

        /// <summary>Add or update the plugins asynchronous.</summary>
        Task<bool> AddOrUpdatePluginsAsync(IReadOnlyList<Plugin> plugins);

        /// <summary>Gets the messages asynchronous.</summary>
        Task<IReadOnlyList<Message>> GetMessagesAsync();

        /// <summary>Saves a message.</summary>
        Task<bool> SaveMessageAsync(Message message);

        /// <summary>Saves a report statistics data.</summary>
        /// <typeparam name="T">The type of the statistic.</typeparam>
        Task<bool> AddOrUpdateStatisticsAsync<T>(Statistics<T> data);

        /// <summary>Add or update the user asynchronous.</summary>
        Task<bool> AddOrUpdateUserAsync(User user);

        /// <summary>Gets all statistics asynchronous.</summary>
        /// <typeparam name="T">Statistic type.</typeparam>
        Task<IReadOnlyList<Statistics<T>>> GetStatisticsAsync<T>(string date, string time);
    }
}
