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
            Contract.Ensures(myTimer != null, "Timer is not instantiated");

            var openAirConnector = context.Get<IOpenAirConnector>();

            await openAirConnector.SyncUsersAsync();
        }

        /// <summary>Execute a timesheet reminder.</summary>
        [Function("timesheets-reminder-configurable")]
        public static async Task ExecuteTimesheetsReminderAsync(
            [TimerTrigger("0 */15 * * * 1-5")] TimerInfo myTimer,
            FunctionContext context)
        {
            Contract.Ensures(myTimer != null, "Timer is not instantiated");

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

            var questions = await req.ReadAsAsync<List<QuestionAnswerViewModel>>();

            AddNewParents(questions);

            var result = questions
                .Select(x => new QuestionAnswer
                {
                    Id = x.Id,
                    AcquireTraits = x.AcquireTraits,
                    RequiredTraits = x.RequiredTraits,
                    Parents = x.Parents,
                    IsAnswer = x.IsAnswer,
                    Content = x.Content,
                    Title = x.Title,
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

                await storageService.DeleteQuestionAnswerAsync(question);

                var children = questions.Where(q => q.Parents.ContainsKey(question.Id)).ToList();

                await AddDeletedQuestionParentsToItsChildren(storageService, question, children);
            }
        }

        private static async Task AddDeletedQuestionParentsToItsChildren(
            IStorageService storageService,
            QuestionAnswer question,
            List<QuestionAnswer> children)
        {
            foreach (var child in children)
            {
                RemoveDeletedParent(question, child);
                foreach (var parent in question.Parents)
                {
                    child.Parents.Add(parent.Key, parent.Value);
                }
            }

            await storageService.AddOrUpdateQuestionsAsync(children);
        }

        private static void RemoveDeletedParent(QuestionAnswer question, QuestionAnswer child)
        {
            child.Parents.Remove(question.Id);
        }

        private static void AddNewParents(List<QuestionAnswerViewModel> questions)
        {
            foreach (var question in questions)
            {
                for (int i = 0; i < question.Parents.Count; i++)
                {
                    if (questions.Any(q => q.Title == question.Parents.ElementAt(i).Key.Trim()))
                    {
                        var parentTitle = question.Parents.ElementAt(i).Key;
                        var parentId = questions.First(q => q.Title == question.Parents.ElementAt(i).Key.Trim()).Id;
                        question.Parents.Remove(parentTitle);
                        question.Parents[parentId] = parentTitle.Trim();
                    }
                }
            }
        }

        private static DateTime GetLocalDateTime(FunctionContext context) =>
            TimeZoneInfo.ConvertTime(
                context.Get<Func<DateTime>>()(),
                context.Get<Func<TimeZoneInfo>>()());
    }
}
