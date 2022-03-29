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
using MentorBot.Functions.Models.ViewModels;

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

            var questions = await req.ReadAsAsync<QuestionAnswerViewModel[]>();

            var questionsToList = questions.ToList();

            var parentIds = new List<string>();

            var index = 1;

            for (int i = 0; i < questionsToList.Count; i++)
            {
                if (!questionsToList[i].Parents.Keys.Any(p => parentIds.FirstOrDefault(pi => pi == p) != null))
                {
                    index = 1;
                    parentIds.AddRange(questionsToList[i].Parents.Keys);
                }

                questionsToList[i].Index = index;
                index++;

                if (questionsToList[i].SubQuestions.Length != 0)
                {
                    var subQuestions = questionsToList[i].SubQuestions;
                    for (int j = 0; j < subQuestions.Length; j++)
                    {
                        subQuestions[j].Index = j + 1;
                    }

                    questionsToList[i].SubQuestions = null;
                    questionsToList.AddRange(subQuestions);
                }
            }

            foreach (var question in questionsToList)
            {
                for (int i = 0; i < question.Parents.Count; i++)
                {
                    if (questionsToList.Any(q => q.Title == question.Parents.ElementAt(i).Key.Trim()))
                    {
                        var parentTitle = question.Parents.ElementAt(i).Key;
                        var parentId = questionsToList.First(q => q.Title == question.Parents.ElementAt(i).Key.Trim()).Id;
                        question.Parents.Remove(parentTitle);
                        question.Parents[parentId] = parentTitle.Trim();
                    }
                }
            }

            var result = questionsToList
                .GroupBy(q => q.Id)
                .Select(q => q.OrderByDescending(q => q.IsEdited))
                .Select(x => new QuestionAnswer
                {
                    Id = x.First().Id,
                    AcquireTraits = x.First().AcquireTraits,
                    RequiredTraits = x.First().RequiredTraits,
                    Parents = x.First().Parents,
                    IsAnswer = x.First().IsAnswer,
                    Content = x.First().Content,
                    Index = x.First().Index,
                    Title = x.First().Title,
                })
                .ToList();

            await storageService.AddOrUpdateQuestionsAsync(result);
        }

        /// <summary>Sets the questions. </summary>
        [Function("delete-question/{id}")]
        public static async Task DeleteQuestionAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Delete), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = context.Get<IStorageService>();

            var questionId = req.Url.AbsolutePath.Split('/').Last();

            if (!string.IsNullOrWhiteSpace(questionId) && questionId != "undefined")
            {
                var questions = await storageService.GetAllQuestionsAsync();

                var question = questions.FirstOrDefault(q => q.Id == questionId);

                var parents = question.Parents;

                var children = questions.Where(q => q.Parents.ContainsKey(question.Id)).ToList();

                await storageService.DeleteQuestionAnswerAsync(question);

                foreach (var child in children)
                {
                    foreach (var parent in parents)
                    {
                        child.Parents.Add(parent.Key, parent.Value);
                    }
                }

                await storageService.AddOrUpdateQuestionsAsync(children);
            }
        }

        private static DateTime GetLocalDateTime(FunctionContext context) =>
            TimeZoneInfo.ConvertTime(
                context.Get<Func<DateTime>>()(),
                context.Get<Func<TimeZoneInfo>>()());
    }
}
