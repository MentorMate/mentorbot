using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;

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

        /// <summary>Execute a timesheet reminder.</summary>
        [Function("timesheets-reminder-configurable")]
        public static async Task ExecuteTimesheetsReminderAsync(
            [TimerTrigger("0 */15 * * * 1-5")] TimerInfo myTimer,
            FunctionContext context)
        {
            Contract.Ensures(myTimer != null, "Timer is not instanciated");

            var timesheetService = context.Get<ITimesheetService>();
            var now = GetLocalDateTime(context);
            var dateTimeMinutes = now.Minute - (now.Minute % 5);
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

        /// <summary>Sets the questions. </summary>
        [Function("save-questions")]
        public static async Task SaveQuestionsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = context.Get<IStorageService>();

            var questions = await req.ReadAsAsync<QuestionAnswer[]>();

            var questionsToList = questions.ToList();

            var parentId = string.Empty;

            var index = 1;

            for (int i = 0; i < questionsToList.Count; i++)
            {
                if (questionsToList[i].ParentId != parentId)
                {
                    index = 1;
                    parentId = questionsToList[i].ParentId;
                }

                questionsToList[i].Index = index.ToString();
                index++;

                if (questionsToList[i].SubQuestions.Length != 0)
                {
                    var subQuestions = questionsToList[i].SubQuestions;
                    for (int j = 0; j < subQuestions.Length; j++)
                    {
                        subQuestions[j].Index = (j + 1).ToString();
                        subQuestions[j].ParentId = questionsToList[i].Id;
                    }

                    questionsToList[i].SubQuestions = null;
                    questionsToList.AddRange(subQuestions);
                }
            }

            await storageService.AddOrUpdateQuestionsAsync(questionsToList);
        }

        private static DateTime GetLocalDateTime(FunctionContext context) =>
            TimeZoneInfo.ConvertTime(
                context.Get<Func<DateTime>>()(),
                context.Get<Func<TimeZoneInfo>>()());
    }
}
