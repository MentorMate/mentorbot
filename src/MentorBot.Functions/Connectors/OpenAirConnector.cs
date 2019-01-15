// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Connectors.OpenAir;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;

namespace MentorBot.Functions.Connectors
{
    /// <summary>Provider methods connected to OpenAir service endpoints.</summary>
    public sealed class OpenAirConnector : IOpenAirConnector
    {
        private readonly OpenAirClient _client;
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="OpenAirConnector"/> class.</summary>
        public OpenAirConnector(OpenAirClient client, IStorageService storageService)
        {
            _client = client;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Timesheet>> GetUnsubmittedTimesheetsAsync(DateTime date)
        {
            var requiredHours = date.DayOfWeek == DayOfWeek.Saturday ? 40 : (int)date.DayOfWeek * 8;
            var toweek = date.AddDays(-(double)date.DayOfWeek);
            var timesheets = await _client.GetTimesheetsAsync(
                toweek.AddDays(-7),
                date.AddDays(1));

            var unsubmittedTimesheets = timesheets
                .Where(it => it.Status == "A" || it.Status == "S")
                .GroupBy(it => it.UserId)
                .Select(it => new
                {
                    UserId = it.Key,
                    Name = it.FirstOrDefault()?.Name,
                    Total = it.Where(sheet => sheet.StartDate.Date > toweek).Sum(sheet => sheet.Total)
                })
                .Where(it => it.Total < requiredHours)
                .ToArray();

            var users = await GetUsersWithDepartmentAsync(unsubmittedTimesheets.Select(it => it.UserId));

            var result = unsubmittedTimesheets
                .Select(it => new { timesheet = it, user = users.FirstOrDefault(user => user.OpenAirUserId == it.UserId) })
                .Where(it => it.user != null)
                .Select(it => new Timesheet
                {
                    Name = it.timesheet.Name,
                    UserName = FormatDisplayName(it.user.Name),
                    UserEmail = it.user.Email,
                    DepartmentName = it.user.Department.Name,
                    Total = it.timesheet.Total
                });

            return result.ToArray();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<User>> GetUsersWithDepartmentAsync(IEnumerable<long> userIdList)
        {
            var usersList = new List<User>(
                await _storageService.GetUsersByIdListAsync(userIdList));

            var usersListIdList = usersList.Select(it => it.OpenAirUserId).ToArray();
            var usersIdListNotStored = userIdList.Where(id => !usersListIdList.Contains(id)).ToArray();
            var usersFromOpenAir = ExecuteAsyncTasks(
                usersIdListNotStored.Select(_client.GetUserByIdAsync),
                CreateUser);

            usersList.AddRange(usersFromOpenAir);

            var usersFromOpenAirWithDepartmentId =
                usersFromOpenAir.Where(it => it.Department.OpenAirDepartmentId.HasValue);

            var departments = ExecuteAsyncTasks(
                usersFromOpenAirWithDepartmentId
                    .Select(it => it.Department.OpenAirDepartmentId.Value)
                    .Distinct()
                    .Select(_client.GetDepartmentByIdAsync),
                CreateDepartment);

            var departmentNames = departments.ToDictionary(it => it.OpenAirDepartmentId, it => it.Name);
            foreach (var user in usersFromOpenAirWithDepartmentId)
            {
                user.Department.Name = departmentNames[user.Department.OpenAirDepartmentId.Value];
            }

            foreach (var user in usersFromOpenAir)
            {
                await _storageService.AddUserAsync(user);
            }

            return usersList;
        }

        private static IReadOnlyList<TDestination> ExecuteAsyncTasks<TSource, TDestination>(
            IEnumerable<Task<TSource>> tasks,
            Func<TSource, TDestination> creator)
        {
            var tasksArray = tasks.ToArray();
            Task.WaitAll(tasksArray);
            return tasksArray.Select(task => creator(task.Result)).ToArray();
        }

        private static User CreateUser(OpenAirClient.User user)
        {
            return new User
            {
                OpenAirUserId = user.Id,
                Name = user.Name,
                Email = user.Address.FirstOrDefault()?.Email,
                Department = new Department
                {
                    OpenAirDepartmentId = user.DepartmentId
                }
            };
        }

        private static Department CreateDepartment(OpenAirClient.Department department)
        {
            return new Department
            {
                OpenAirDepartmentId = department.Id,
                Name = department.Name
            };
        }

        private static string FormatDisplayName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var names = name.Split(',');
            return names.Length == 1 ? name : $"{names[1].Trim()} {names[0].Trim()}";
        }
    }
}
