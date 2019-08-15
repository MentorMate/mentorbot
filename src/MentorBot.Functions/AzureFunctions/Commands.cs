// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Settings;
using MentorBot.Functions.Processors;

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
        [Disable]
        public static async Task TimesheetsReminderAsync(
            [TimerTrigger("0 */60 18-19 * * Fri")] TimerInfo myTimer)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            var storage = ServiceLocator.Get<IStorageService>();
            var connector = ServiceLocator.Get<IHangoutsChatConnector>();
            var processor = ServiceLocator.Get<ITimesheetProcessor>();

            var settings = await storage.GetSettingsAsync();
            var data = settings.Processors.FirstOrDefault(it => it.Name == nameof(OpenAirProcessor))?.DataAsDictionary();

            await processor.NotifyAsync(
                DateTime.Today,
                TimesheetStates.Unsubmitted,
                data?.GetValueOrDefault(Default.EmailKey),
                data?.GetAsArray(Default.DefaultExcludedClientKey),
                null,
                true,
                data?.GetValueOrDefault(Default.NotifyByEmailKey) == "true",
                null,
                connector);
        }

        /// <summary>A sync users command.</summary>
        [FunctionName("timesheets-reminder-last-week")]
        [Disable]
        public static async Task TimesheetsReminderLastWeekAsync(
            [TimerTrigger("0 0 12 * * Mon")] TimerInfo myTimer)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            ServiceLocator.EnsureServiceProvider();

            var storage = ServiceLocator.Get<IStorageService>();
            var connector = ServiceLocator.Get<IHangoutsChatConnector>();
            var processor = ServiceLocator.Get<ITimesheetProcessor>();

            var settings = await storage.GetSettingsAsync();
            var data = settings.Processors.FirstOrDefault(it => it.Name == nameof(OpenAirProcessor))?.DataAsDictionary();
            var lastWeekFriday = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek + 2));

            await processor.NotifyAsync(
                lastWeekFriday,
                TimesheetStates.Unsubmitted,
                data?.GetValueOrDefault(Default.EmailKey),
                data?.GetAsArray(Default.DefaultExcludedClientKey),
                null,
                true,
                data?.GetValueOrDefault(Default.NotifyByEmailKey) == "true",
                null,
                connector);

            await processor.NotifyAsync(
                lastWeekFriday,
                TimesheetStates.Unapproved,
                data?.GetValueOrDefault(Default.EmailKey),
                data?.GetAsArray(Default.DefaultExcludedClientKey),
                null,
                true,
                data?.GetValueOrDefault(Default.NotifyByEmailKey) == "true",
                null,
                connector);
        }

        /// <summary>Sets the MentorBot settings to storage.</summary>
        [FunctionName("save-settings")]
        public static async Task SaveSettingsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = null)] HttpRequest req)
        {
            ServiceLocator.EnsureServiceProvider();

            await ServiceLocator.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = ServiceLocator.Get<IStorageService>();

            ServiceLocator.Get<IMemoryCache>().Remove(Constants.SettingsCacheKey);

            var settings = new MentorBotSettings
            {
                Processors = await GetBodyAsync(req),
            };

            await storageService.SaveSettingsAsync(settings);
        }

        private static async Task<IReadOnlyList<ProcessorSettings>> GetBodyAsync(HttpRequest req)
        {
            var body = req.Body ?? throw new ArgumentNullException(nameof(req));

            using (StreamReader reader = new StreamReader(body))
            {
                var requestBody = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<List<ProcessorSettings>>(requestBody);
            }
        }
    }
}
