// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.Timesheets;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Caching.Memory;

using Newtonsoft.Json;

namespace MentorBot.Functions
{
    /// <summary>Application query functions.</summary>
    public static class Commands
    {
        /// <summary>A sync users command.</summary>
        [FunctionName("sync-users")]
        public static async Task SyncUsersAsync(
            [TimerTrigger("0 0 9 * * Fri")] TimerInfo myTimer)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            var openAirConnector = ServiceLocator.Get<IOpenAirConnector>();

            await openAirConnector.SyncUsersAsync();
        }

        /// <summary>A sync users command.</summary>
        [FunctionName("timesheets-reminder")]
        public static async Task TimesheetsReminderAsync(
            [TimerTrigger("0 */60 18-19 * * Fri")] TimerInfo myTimer)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            var cognitiveService = ServiceLocator.Get<ICognitiveService>();
            var connector = ServiceLocator.Get<IHangoutsChatConnector>();
            var processor = ServiceLocator.Get<ITimesheetProcessor>();

            var result = await cognitiveService.GetCognitiveTextAnalysisResultAsync(
                new TextDeconstructionInformation(string.Empty, "Timesheets"), null);

            var excludes = result.PropertiesAccessor.GetAllPluginPropertyValues<string>(TimesheetsProperties.FilterByCustomer);
            var groups = result.PropertiesAccessor.GetPluginPropertyGroup(TimesheetsProperties.NotificationsGroup);
            foreach (var group in groups)
            {
                var email = group.GetValue<string>(TimesheetsProperties.Email);
                var notify = group.GetValue<bool>(TimesheetsProperties.NotifyByEmail);
                var filterOutEmail = group.GetValue<bool>(TimesheetsProperties.DontNotifyManager);

                await processor.NotifyAsync(
                    DateTime.Today,
                    TimesheetStates.Unsubmitted,
                    email,
                    excludes,
                    null,
                    true,
                    notify,
                    filterOutEmail,
                    null,
                    connector);
            }
        }

        /// <summary>Execute a timesheet reminder.</summary>
        [FunctionName("timesheets-reminder-configurable")]
        public static async Task ExecuteTimesheetsReminderAsync(
            [TimerTrigger("0 */30 * * * 1-5")] TimerInfo myTimer)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            var timesheetService = ServiceLocator.Get<ITimesheetService>();
            var now = DateTime.Now;
            var dateTimeMinutes = (now.Minute / 10) * 10;
            var dateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, dateTimeMinutes, 0, 0, now.Kind);

            await timesheetService.SendScheduledTimesheetNotificationsAsync(dateTime);
        }

        /// <summary>Sets the MentorBot plugins to storage.</summary>
        [FunctionName("save-plugins")]
        public static async Task SavePluginsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = null)] HttpRequest req)
        {
            ServiceLocator.EnsureServiceProvider();

            await ServiceLocator.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = ServiceLocator.Get<IStorageService>();

            ServiceLocator.Get<IMemoryCache>().Remove(Constants.PluginsCacheKey);

            IReadOnlyList<Plugin> plugins = await GetBodyAsync<List<Plugin>>(req);

            await storageService.AddOrUpdatePluginsAsync(plugins);
        }

        /// <summary>Sets the MentorBot plugins to storage.</summary>
        [FunctionName("save-user-props")]
        public static async Task SaveUserPropertiesAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = null)] HttpRequest req)
        {
            ServiceLocator.EnsureServiceProvider();

            await ServiceLocator.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = ServiceLocator.Get<IStorageService>();

            var userInfo = await GetBodyAsync<UserInfo>(req);

            var user = await storageService.GetUserByEmailAsync(userInfo.Email);

            user.Properties = userInfo.Properties;

            await storageService.AddOrUpdateUserAsync(user);
        }

        private static async Task<T> GetBodyAsync<T>(HttpRequest req)
        {
            var body = req.Body ?? throw new ArgumentNullException(nameof(req));

            using (var reader = new StreamReader(body))
            {
                var requestBody = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(requestBody);
            }
        }
    }
}
