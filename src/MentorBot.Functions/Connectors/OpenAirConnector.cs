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
using MentorBot.Functions.Models.Domains.Base;

namespace MentorBot.Functions.Connectors
{
    /// <summary>Provider methods connected to OpenAir service endpoints.</summary>
    public sealed class OpenAirConnector : IOpenAirConnector
    {
        private readonly IOpenAirClient _client;
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="OpenAirConnector"/> class.</summary>
        public OpenAirConnector(IOpenAirClient client, IStorageService storageService)
        {
            _client = client;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Timesheet>> GetUnsubmittedTimesheetsAsync(DateTime date, string[] filterByCustomers)
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
                    TimesheetName = it.FirstOrDefault()?.Name,
                    Total = it.Where(sheet => sheet.StartDate.Date > toweek).Sum(sheet => sheet.Total)
                })
                .Where(it => it.Total < requiredHours)
                .ToArray();

            var users = _storageService.GetUsersByIdList(
                unsubmittedTimesheets.Select(it => it.UserId.Value).Distinct().ToArray());

            if (filterByCustomers != null && filterByCustomers.Any())
            {
                var normalizedCustomerNames = filterByCustomers.Select(NormalizeValue).ToArray();
                users = users.Where(it => it.Customers == null || FiterCustomersByNames(it.Customers, normalizedCustomerNames)).ToArray();
            }

            var result = unsubmittedTimesheets
                .Select(it => new { timesheet = it, user = users.FirstOrDefault(user => user.OpenAirUserId == it.UserId) })
                .Where(it => it.user != null && it.user.Active)
                .Select(it => new Timesheet
                {
                    Name = it.timesheet.TimesheetName,
                    UserName = FormatDisplayName(it.user.Name),
                    UserEmail = it.user.Email,
                    UserManagerEmail = it.user.Manager?.Email,
                    DepartmentName = it.user.Department.Name,
                    DepartmentOwnerEmail = it.user.Department.Owner?.Email,
                    Total = it.timesheet.Total.Value
                });

            return result.ToArray();
        }

        /// <inheritdoc/>
        public async Task SyncUsersAsync()
        {
            var storedUsers = _storageService.GetAllUsers();
            var openAirModelUsers = await _client.GetAllUsersAsync();
            var openAirDepartments = await _client.GetAllDepartmentsAsync();
            var openAirCustomers = await _client.GetAllActiveCustomersAsync();
            var openAirBookings = await _client.GetAllActiveBookingsAsync(DateTime.Today);
            var usersListToUpdate = new List<User>();
            var usersListToAdd = new List<User>();
            foreach (var user in openAirModelUsers)
            {
                var storedUser = storedUsers.FirstOrDefault(it => it.OpenAirUserId == user.Id);
                var department = user.DepartmentId.HasValue ?
                    CreateDepartment(openAirDepartments.FirstOrDefault(it => it.Id == user.DepartmentId.Value), openAirModelUsers) :
                    null;

                var manager = CreateUserReferenceById(user.ManagerId, openAirModelUsers);

                var customerIdList = openAirBookings
                    .Where(it => it.UserId == user.Id)
                    .Select(it => it.CustomerId)
                    .Distinct()
                    .ToArray();

                var customers = openAirCustomers
                    .Where(it => customerIdList.Contains(it.Id))
                    .Select(it => new Customer { OpenAirId = it.Id.Value, Name = it.Name })
                    .ToArray();

                if (storedUser == null && user.Active == true)
                {
                    var createUser = CreateUser(null, user, manager, department, customers);
                    usersListToAdd.Add(createUser);
                }
                else if (storedUser != null && UserNeedUpdate(storedUser, user, manager, department, customers))
                {
                    var updateUser = CreateUser(storedUser.Id, user, manager, department, customers);
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

        private static bool UserNeedUpdate(User storedUser, OpenAirClient.User openAirUser, UserReference manager, Department department, Customer[] openAirCustomers) =>
            storedUser.Active != openAirUser.Active ||
            storedUser.Department?.OpenAirDepartmentId != openAirUser.DepartmentId ||
            storedUser.Department?.Owner?.OpenAirUserId != department?.Owner?.OpenAirUserId ||
            storedUser.Manager?.OpenAirUserId != manager.OpenAirUserId ||
            storedUser.Customers?.Length != openAirCustomers.Length ||
            openAirCustomers.Any(it => !storedUser.Customers.Contains(it));

        private static bool FiterCustomersByNames(Customer[] customers, string[] customerNames) =>
            !customers.Any(customer => InArray(NormalizeValue(customer.Name), customerNames));

        private static bool InArray(string value, string[] values) =>
            values.Any(val2 => val2.Equals(value, StringComparison.InvariantCultureIgnoreCase));

        private static string NormalizeValue(string value) =>
            value.Replace(" ", string.Empty, StringComparison.InvariantCulture);

        private static User CreateUser(Guid? id, OpenAirClient.User user, UserReference manager, Department department, Customer[] customers) =>
            new User
            {
                Id = id,
                OpenAirUserId = user.Id.Value,
                Name = user.Name,
                Email = user.Address.FirstOrDefault()?.Email,
                Active = user.Active ?? false,
                Department = department,
                Manager = manager,
                Customers = customers
            };

        private static UserReference CreateUserReferenceById(long? userId, OpenAirClient.User[] users) =>
            userId.HasValue ? CreateUserReference(users.FirstOrDefault(it => it.Id == userId.Value)) : null;

        private static UserReference CreateUserReference(OpenAirClient.User user) =>
            user == null ?
            null :
            new UserReference
            {
                OpenAirUserId = user.Id.Value,
                Email = user.Address.FirstOrDefault()?.Email
            };

        private static Department CreateDepartment(OpenAirClient.Department department, OpenAirClient.User[] users) =>
            department == null ? null :
            new Department
            {
                OpenAirDepartmentId = department.Id,
                Name = department.Name,
                Owner = CreateUserReferenceById(department.UserId, users)
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
