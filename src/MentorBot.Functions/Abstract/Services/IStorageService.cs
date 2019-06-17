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

        /// <summary>Adds the addresses asynchronous.</summary>
        Task<bool> AddAddressesAsync(IReadOnlyList<GoogleAddress> addresses);

        /// <summary>Gets all stored active users.</summary>
        Task<IReadOnlyList<User>> GetAllActiveUsersAsync();

        /// <summary>Gets all stored users.</summary>
        Task<IReadOnlyList<User>> GetAllUsersAsync();

        /// <summary>Adds the users asynchronous.</summary>
        Task<bool> AddUsersAsync(IReadOnlyList<User> users);

        /// <summary>Update the users asynchronous.</summary>
        Task<bool> UpdateUsersAsync(IReadOnlyList<User> users);

        /// <summary>Gets the messages asynchronous.</summary>
        Task<IReadOnlyList<Message>> GetMessagesAsync();

        /// <summary>Saves a message.</summary>
        Task<bool> SaveMessageAsync(Message message);

        /// <summary>Reads the settings from the Table Storage.</summary>
        Task<MentorBotSettings> GetSettingsAsync();

        /// <summary>Saves the passed settings asynchronius.</summary>
        /// <param name="settings">An object that contains the latest settings.</param>
        Task<bool> SaveSettingsAsync(MentorBotSettings settings);
    }
}
