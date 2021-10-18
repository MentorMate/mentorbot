using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.Timesheets;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Memory;

namespace MentorBot.Functions
{
    /// <summary>Application query functions.</summary>
    public static class Commands
    {
        /// <summary>A sync users command.</summary>
        [Function("sync-users")]
        public static async Task SyncUsersAsync(
            [TimerTrigger("0 0 9 * * Fri")] TimerInfo myTimer,
            FunctionContext context)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            var openAirConnector = context.Get<IOpenAirConnector>();

            await openAirConnector.SyncUsersAsync();
        }

        /// <summary>A sync users command.</summary>
        [Function("timesheets-reminder")]
        public static async Task TimesheetsReminderAsync(
            [TimerTrigger("0 */60 18-19 * * Fri")] TimerInfo myTimer,
            FunctionContext context)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            var cognitiveService = context.Get<ICognitiveService>();
            var connector = context.Get<IHangoutsChatConnector>();
            var processor = context.Get<ITimesheetProcessor>();

            var result = await cognitiveService.GetCognitiveTextAnalysisResultAsync(
                new TextDeconstructionInformation(string.Empty, "Timesheets"), null);

            var excludes = result.PropertiesAccessor.GetAllPluginPropertyValues<string>(TimesheetsProperties.FilterByCustomer);
            var groups = result.PropertiesAccessor.GetPluginPropertyGroup(TimesheetsProperties.NotificationsGroup);
            var today = GetLocalDateTime(context).Date;
            foreach (var group in groups)
            {
                var email = group.GetValue<string>(TimesheetsProperties.Email);
                var notify = group.GetValue<bool>(TimesheetsProperties.NotifyByEmail);
                var filterOutEmail = group.GetValue<bool>(TimesheetsProperties.DoNotNotifyManager);

                await processor.NotifyAsync(
                    today,
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
        [Function("timesheets-reminder-configurable")]
        public static async Task ExecuteTimesheetsReminderAsync(
            [TimerTrigger("0 */30 * * * 1-5")] TimerInfo myTimer,
            FunctionContext context)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            var timesheetService = context.Get<ITimesheetService>();
            var now = GetLocalDateTime(context);
            var dateTimeMinutes = (now.Minute / 10) * 10;
            var dateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, dateTimeMinutes, 0, 0, now.Kind);

            await timesheetService.SendScheduledTimesheetNotificationsAsync(dateTime);
        }

        /// <summary>Sets the MentorBot plugins to storage.</summary>
        [Function("save-plugins")]
        public static async Task SavePluginsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = context.Get<IStorageService>();

            context.Get<IMemoryCache>().Remove(Constants.PluginsCacheKey);

            var plugins = await req.ReadAsAsync<IReadOnlyList<Plugin>>();

            await storageService.AddOrUpdatePluginsAsync(plugins);
        }

        /// <summary>Sets the MentorBot plugins to storage.</summary>
        [Function("save-user-props")]
        public static async Task SaveUserPropertiesAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = context.Get<IStorageService>();

            var userInfo = await req.ReadAsAsync<UserInfo>();

            var user = await storageService.GetUserByIdAsync(userInfo.Id);

            user.Properties = userInfo.Properties;

            await storageService.AddOrUpdateUserAsync(user);
        }

        private static DateTime GetLocalDateTime(FunctionContext context) =>
            TimeZoneInfo.ConvertTime(
                context.Get<Func<DateTime>>()(),
                context.Get<Func<TimeZoneInfo>>()());
    }
}
