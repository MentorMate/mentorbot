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
        public async Task<IReadOnlyList<Timesheet>> GetUnsubmittedTimesheetsAsync(DateTime date, string[] filterByProjects)
        {
            var requiredHours = date.DayOfWeek == DayOfWeek.Saturday ? 40 : (int)date.DayOfWeek * 8;
            var toweek = date.AddDays(-(double)date.DayOfWeek);
            var lastWeek = toweek.AddDays(-7);
            var timesheets = new List<OpenAirClient.Timesheet>();

            timesheets.AddRange(await _client.GetTimesheetsAsync(lastWeek, lastWeek.AddDays(2)));
            timesheets.AddRange(await _client.GetTimesheetsAsync(toweek, date.AddDays(1)));

            var unsubmittedTimesheets = timesheets
                .GroupBy(it => it.UserId)
                .Select(it => new
                {
                    UserId = it.Key,
                    Name = it.FirstOrDefault()?.Name,
                    Total = it.Where(sheet => sheet.StartDate.Date > toweek).Sum(sheet => sheet.Total)
                })
                .Where(it => it.Total < requiredHours)
                .ToArray();

            var users = _storageService.GetUsersByIdList(
                unsubmittedTimesheets.Select(it => it.UserId.Value).Distinct().ToArray());

            if (filterByProjects != null && filterByProjects.Any())
            {
                users = users.Where(it => it.Projects == null || FiterProjectsByNames(it.Projects, filterByProjects)).ToArray();
            }

            var result = unsubmittedTimesheets
                .Select(it => new { timesheet = it, user = users.FirstOrDefault(user => user.OpenAirUserId == it.UserId) })
                .Where(it => it.user != null && it.user.Active)
                .Select(it => new Timesheet
                {
                    Name = it.timesheet.Name,
                    UserName = FormatDisplayName(it.user.Name),
                    UserEmail = it.user.Email,
                    DepartmentName = it.user.Department.Name,
                    Total = it.timesheet.Total.Value
                });

            return result.ToArray();
        }

        /// <inheritdoc/>
        public async Task SyncUsersAsync()
        {
            var storedUsers = _storageService.GetAllUsers();
            var openAirModelUsers = await _client.GetAllUserAsync();
            var openAirDepartments = await _client.GetAllDepartmentsAsync();
            var usersListToUpdate = new List<User>();
            var usersListToAdd = new List<User>();
            foreach (var user in openAirModelUsers)
            {
                var storedUser = storedUsers.FirstOrDefault(it => it.OpenAirUserId == user.Id);
                var department = openAirDepartments.FirstOrDefault(it => it.Id == user.DepartmentId);
                if (storedUser == null && user.Active == true)
                {
                    var createUser = CreateUser(null, user, department, null);
                    usersListToAdd.Add(createUser);
                }
                else if (storedUser != null &&
                    (storedUser.Active != user.Active
                    || storedUser.Department.OpenAirDepartmentId != user.DepartmentId))
                {
                    var updateUser = CreateUser(storedUser.Id, user, department, null);
                    usersListToUpdate.Add(updateUser);
                }
            }

            if (usersListToAdd.Count > 0)
            {
                await _storageService.AddUsersAsync(usersListToAdd);
            }

            if (usersListToUpdate.Count > 0)
            {
                await _storageService.UpdateUsersAsync(usersListToUpdate);
            }
        }

        private static bool FiterProjectsByNames(Project[] projects, string[] projectNames) =>
            !projects.Any(it => projectNames.Any(name => name.Equals(it.Name, StringComparison.InvariantCultureIgnoreCase)));

        private static User CreateUser(Guid? id, OpenAirClient.User user, OpenAirClient.Department department, Project[] projects) =>
            new User
            {
                Id = id,
                OpenAirUserId = user.Id,
                Name = user.Name,
                Email = user.Address.FirstOrDefault()?.Email,
                Active = user.Active ?? false,
                Department = CreateDepartment(department),
                Projects = projects
            };

        private static Department CreateDepartment(OpenAirClient.Department department) =>
            new Department
            {
                OpenAirDepartmentId = department.Id,
                Name = department.Name
            };

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
