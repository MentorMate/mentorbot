// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Settings;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace MentorBot.Functions
{
    /// <summary>Application query functions.</summary>
    public static class Queries
    {
        /// <summary>Get the messages statistics asynchronous.</summary>
        [FunctionName("get-messages-stats")]
        public static async Task<IEnumerable<MessagesStatistic>> GetMessagesStatisticsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethods.Get), Route = null)] HttpRequest req)
        {
            Contract.Ensures(req != null, "Request is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            await ServiceLocator.Get<IAccessTokenService>().EnsureRole(req, UserRoles.User | UserRoles.Administrator);

            var storage = ServiceLocator.Get<IStorageService>() ?? throw new NullReferenceException();

            var messages = (await storage.GetMessagesAsync())
                .GroupBy(it => it.ProbabilityPercentage / 10)
                .Select(group => new MessagesStatistic
                {
                    ProbabilityPercentage = (byte)(group.Key * 10),
                    Count = group.Count()
                });

            return messages;
        }

        /// <summary>Gets the settings asynchronous.</summary>
        [FunctionName("get-settings")]
        public static async Task<IEnumerable<ProcessorSettings>> GetSettingsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethods.Get), Route = null)] HttpRequest req)
        {
            Contract.Ensures(req != null, "Request is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            await ServiceLocator.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storage = ServiceLocator.Get<IStorageService>() ?? throw new NullReferenceException();

            var settings = await storage.GetSettingsAsync();

            return settings?.Processors;
        }

        /// <summary>Gets user auth info asynchronous.</summary>
        [FunctionName("get-user-info")]
        public static async Task<AccessTokenUserInfo> GetUserInfoAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethods.Get), Route = null)] HttpRequest req)
        {
            Contract.Ensures(req != null, "Request is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            var accessTokenService = ServiceLocator.Get<IAccessTokenService>() ?? throw new NullReferenceException();

            var userInfo = await accessTokenService.ValidateTokenAsync(req);

            return userInfo;
        }

        /// <summary>Gets users info asynchronous.</summary>
        [FunctionName("get-users")]
        public static async Task<IEnumerable<UserInfo>> GetUsersAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethods.Get), Route = null)] HttpRequest req)
        {
            Contract.Ensures(req != null, "Request is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            await ServiceLocator.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = ServiceLocator.Get<IStorageService>() ?? throw new NullReferenceException();

            var users = await storageService.GetAllActiveUsersAsync();

            return users.Select(it => new UserInfo
            {
                Id = it.Id,
                Name = it.Name,
                Email = it.Email,
                Manager = it.Manager?.Email,
                Department = it.Department?.Name,
                Role = Enum.GetName(typeof(UserRoles), it.Role == default ? UserRoles.User : (UserRoles)it.Role),
                Customers = string.Join(", ", it.Customers?.Select(cust => cust.Name)),
            });
        }
    }
}
