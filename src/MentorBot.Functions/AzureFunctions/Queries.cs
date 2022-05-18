using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.AzureFunctions;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.ViewModels;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MentorBot.Functions
{
    /// <summary>Application query functions.</summary>
    public static class Queries
    {
        /// <summary>Get the messages statistics asynchronous.</summary>
        [Function("get-messages-stats")]
        public static async Task<IEnumerable<MessagesStatistic>> GetMessagesStatisticsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Get), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            Contract.Ensures(req != null, "Request is not instantiated");

            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.User | UserRoles.Administrator);

            var storage = context.Get<IStorageService>() ?? throw new NullReferenceException();

            var messages = (await storage.GetMessagesAsync())
                .GroupBy(it => it.ProbabilityPercentage / 10)
                .Select(group => new MessagesStatistic
                {
                    ProbabilityPercentage = (byte)(group.Key * 10),
                    Count = group.Count()
                });

            return messages;
        }

        /// <summary>Gets user auth info asynchronous.</summary>
        [Function("get-user-info")]
        public static async Task<AccessTokenUserInfo> GetUserInfoAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Get), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            Contract.Ensures(req != null, "Request is not instantiated");

            var accessTokenService = context.Get<IAccessTokenService>() ?? throw new NullReferenceException();

            var userInfo = await accessTokenService.ValidateTokenAsync(req);

            return userInfo;
        }

        /// <summary>Gets users info asynchronous.</summary>
        [Function("get-users")]
        public static async Task<IEnumerable<UserInfo>> GetUsersAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Get), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            Contract.Ensures(req != null, "Request is not instantiated");

            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storageService = context.Get<IStorageService>() ?? throw new NullReferenceException();

            var users = await storageService.GetAllActiveUsersAsync();

            return users.Select(it => new UserInfo
            {
                Id = it.Id,
                Name = it.Name,
                Email = it.Email,
                Manager = it.Manager?.Email,
                Department = it.Department?.Name,
                Role = Enum.GetName(typeof(UserRoles), it.Role),
                Customers = string.Join(", ", it.Customers?.Select(customer => customer.Name) ?? Enumerable.Empty<string>()),
                Properties = it.Properties,
            });
        }

        /// <summary>Gets the settings asynchronous.</summary>
        [Function("get-plugins")]
        public static async Task<IEnumerable<Plugin>> GetPluginsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Get), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            Contract.Ensures(req != null, "Request is not instantiated");

            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.Administrator);

            var storage = context.Get<IStorageService>() ?? throw new NullReferenceException();

            var plugins = await storage.GetAllPluginsAsync();
            var systemPlugins = SystemPlugins.GetSystemPlugins();
            if (!plugins?.Any() ?? false)
            {
                await storage.AddOrUpdatePluginsAsync(systemPlugins);
                return systemPlugins;
            }

            var hasMissingGroups = false;
            var pluginsIds = plugins.Select(it => it.Id).ToArray();
            var missingPlugins = systemPlugins.Where(it => !pluginsIds.Contains(it.Id)).ToArray();
            var combinedPlugins = plugins
                .Join(
                    systemPlugins,
                    it => it.Id,
                    it => it.Id,
                    (p, ps) =>
                    {
                        if (p.Groups == null)
                        {
                            hasMissingGroups = true;
                            p.Groups = ps.Groups;
                            return p;
                        }

                        var pluginGroupsCount = p.Groups.Length;
                        var systemPluginsGroupCount = ps.Groups?.Length ?? 0;
                        if (pluginGroupsCount < systemPluginsGroupCount)
                        {
                            hasMissingGroups = true;
                            var names = p.Groups.Select(it => it.UniqueName).ToArray();
                            p.Groups = p.Groups.Concat(ps.Groups.Where(it => !names.Contains(it.UniqueName))).ToArray();
                        }

                        return p;
                    })
                .Concat(missingPlugins)
                .ToArray();

            if (missingPlugins.Any() || hasMissingGroups)
            {
                await storage.AddOrUpdatePluginsAsync(combinedPlugins);
            }

            return combinedPlugins;
        }

        /// <summary>Gets users info asynchronous.</summary>
        [Function("get-timesheet-stats")]
        public static async Task<IEnumerable<TimesheetChartStatistic>> GetTimesheetStatisticsAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Get), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            Contract.Ensures(req != null, "Request is not instantiated");

            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.User | UserRoles.Administrator);

            var storageService = context.Get<IStorageService>() ?? throw new NullReferenceException();

            const int hour = 20;
            const DayOfWeek reportDayOfWeek = DayOfWeek.Friday;
            var dataCount = 10;
            var startDate = GetLastDateTime(Contract.LocalDateTime, reportDayOfWeek, hour);
            var data = new List<TimesheetChartStatistic>();

            while (dataCount-- > 0)
            {
                var dateValue = startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var timeValue = startDate.ToString("HH:00", CultureInfo.InvariantCulture);
                var stats = await storageService.GetStatisticsAsync<TimesheetStatistics[]>(dateValue, timeValue);
                var chartStats = stats
                    .SelectMany(it => it.Data)
                    .Where(it => it.State == TimesheetStates.Unsubmitted)
                    .GroupBy(it => it.DepartmentName)
                    .Select(group => new TimesheetChartStatistic
                    {
                        Date = dateValue,
                        Department = group.Key,
                        Count = group.Count(),
                    });

                data.AddRange(chartStats);
                startDate = startDate.AddDays(-7);
            }

            data.Reverse();

            return data;
        }

        /// <summary>Gets all questions and answers asynchronous.</summary>
        [Function("get-questions")]
        public static async Task<IEnumerable<QuestionAnswerViewModel>> GetQuestionsAnswersAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Get), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            Contract.Ensures(req != null, "Request is not instantiated");

            await context.Get<IAccessTokenService>().EnsureRole(req, UserRoles.User | UserRoles.Administrator);

            var storageService = context.Get<IStorageService>() ?? throw new NullReferenceException();

            var data = await storageService.GetAllQuestionsAsync();

            var questionFactory = QuestionFactory.Create(data);

            var questionViewModels = questionFactory.QuestionAnswers;

            var result = questionViewModels.ToList();

            var questionsToDelete = new List<QuestionAnswerViewModel>();
            NestData(questionViewModels, result, questionsToDelete);

            result.RemoveAll(q => questionsToDelete.Any(qtd => qtd.Id == q.Id));

            return result;
        }

        private static void NestData(
            IEnumerable<QuestionAnswerViewModel> questionViewModels,
            List<QuestionAnswerViewModel> result,
            List<QuestionAnswerViewModel> questionsToDelete)
        {
            foreach (var question in questionViewModels)
            {
                if (question.SubQuestions != null && question.SubQuestions.Length
                    != 0 && result.Where(q => q.SubQuestions != null)
                    .Any(q => q.SubQuestions.Any(sq => sq.Parents != null && sq.Parents.Keys.Contains(question.Id))))
                {
                    result.First(q => q.Id == question.Id).SubQuestions =
                        result.Where(q => q.Parents != null && q.Parents.Keys.Contains(question.Id)).ToArray();

                    questionsToDelete.AddRange(result.Where(q => q.Parents != null && q.Parents.Keys.Contains(question.Id)));
                }
            }
        }

        private static DateTime GetLastDateTime(DateTime now, DayOfWeek dayOfWeek, int hour)
        {
            var startDateTime = now.Hour < hour ? now.AddDays(-1) : now;
            var lastDayOfWeek = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, hour, 0, 0, 0, now.Kind);
            while (lastDayOfWeek.DayOfWeek != dayOfWeek)
            {
                lastDayOfWeek = lastDayOfWeek.AddDays(-1);
            }

            return lastDayOfWeek;
        }
    }
}
