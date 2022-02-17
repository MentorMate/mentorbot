// cSpell:ignore ownerid, projectid, customerid, departmentid, locationid, 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.OpenAir;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Base;
using MentorBot.Functions.Models.Options;
using MentorBot.Tests.Base;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    /// <summary>Tests for <see cref="OpenAirConnector" />.</summary>
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class OpenAirConnectorTests
    {
        [TestMethod]
        public void OpenAirClientUser_ShouldBeSet()
        {
            var user = new OpenAirClient.User { IdAsText = "3", ManagerIdAsText = "4", DepartmentIdAsText = "5", ActiveAsText = "1", LocationIdAsText = "6" };
            Assert.AreEqual(3, user.Id);
            Assert.AreEqual(4, user.ManagerId);
            Assert.AreEqual(5, user.DepartmentId);
            Assert.AreEqual(6, user.LocationId);
            Assert.IsTrue(user.Active.Value);
        }

        [TestMethod]
        public void OpenAirClientCustomer_ShouldBeSet()
        {
            var customer = new OpenAirClient.Customer { IdAsText = "100", ActiveAsText = string.Empty, Name = "A" };
            Assert.AreEqual(100, customer.Id);
            Assert.AreEqual("A", customer.Name);
            Assert.IsFalse(customer.Active.Value);
        }

        [TestMethod]
        public void OpenAirClientDepartment_ShouldBeSet()
        {
            var dep = new OpenAirClient.Department { UserIdAsText = "200", Name = "B" };
            Assert.AreEqual(200, dep.UserId);
            Assert.AreEqual("B", dep.Name);
        }

        [TestMethod]
        public void OpenAirClientDate_ShouldCompare()
        {
            var dateTime = new DateTime(2019, 10, 10);
            var date = new OpenAirClient.Date { Year = 2019, Month = 10, Day = 10 };
            Assert.AreEqual(0, date.CompareTo(dateTime));
            Assert.AreEqual(-1, date.CompareTo(new DateTime(2019, 11, 10)));
            Assert.IsTrue(date.Equals(dateTime));
            Assert.IsTrue(date == dateTime);
            Assert.IsTrue(date >= dateTime);
            Assert.IsTrue(date <= dateTime);
            Assert.IsTrue(date > new DateTime(2019, 10, 9));
            Assert.IsTrue(date < new DateTime(2019, 11, 10));
            Assert.IsFalse(date != dateTime);
        }

        [TestMethod]
        public async Task OpenAirClientGetTimesheets_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Timesheet><status>A</status><userid>722</userid><name>02/25/19 to 03/03/19 PTO</name><total>8.00</total><starts><Date><hour/><minute/><timezone/><second/><month>02</month><day>25</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var date = new DateTime(2000, 10, 10);
            var results = await client.GetTimesheetsAsync(date, date);
            var content = Encoding.UTF8.GetString(handler[0].RequestContent);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Timesheet\" filter=\"newer-than,older-than\" field=\"starts,starts\" method=\"all\" limit=\"0,1000\"><Date><month>10</month><day>10</day><year>2000</year></Date><Date><month>10</month><day>10</day><year>2000</year></Date><_Return><status/><name /><total/><notes /><userid /><starts /></_Return></Read></request>", content);
            Assert.AreEqual(1, results.Count);

            var first = results.First();
            Assert.AreEqual(722, first.UserId);
            Assert.AreEqual(2, first.StartDate.Date.Month);
            Assert.AreEqual(25, first.StartDate.Date.Day);
            Assert.AreEqual(2019, first.StartDate.Date.Year);
        }

        [TestMethod]
        public async Task OpenAirClientGetAllUsers_ShouldParseRecord()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var record =
                "<User><departmentid>2</departmentid><name>Oniyide, Temitope</name><id>1623</id><active>1</active>" +
                "<user_locationid>3</user_locationid><line_managerid>317</line_managerid><addr><Address>" +
                "<email>temitope.oniyide@mentormate.com</email><first>Temitope</first><last>Oniyide</last></Address></addr>" +
                "<usr_start_date__c>2021-11-11</usr_start_date__c></User>";
            var handler = new MockHttpMessageHandler()
                .Set($"<response><Auth status=\"0\"></Auth ><Read status=\"0\">{record}</Read ></response>");

            var client = new OpenAirClient(() => handler, options);

            var results = await client.GetAllUsersAsync();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(2, results.Single().DepartmentId);
            Assert.AreEqual(3, results.Single().LocationId);
            Assert.AreEqual(317, results.Single().ManagerId);
            Assert.AreEqual("temitope.oniyide@mentormate.com", results.Single().Address.Single().Email);
            Assert.AreEqual("11/11/2021 00:00", results.Single().StartDate.Value.ToString("g", CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public async Task OpenAirClientGetAllUsers_ShouldParseInvalidStartDate()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var record =
                "<User><departmentid>2</departmentid><name>Oniyide, Temitope</name><id>1623</id><active>1</active>" +
                "<usr_start_date__c>0000-00-00</usr_start_date__c></User>";
            var handler = new MockHttpMessageHandler()
                .Set($"<response><Auth status=\"0\"></Auth ><Read status=\"0\">{record}</Read ></response>");

            var client = new OpenAirClient(() => handler, options);

            var results = await client.GetAllUsersAsync();
            Assert.IsNull(results.Single().StartDate);
        }

        [TestMethod]
        public async Task OpenAirClientGetTimesheetsByStatus_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var date = new DateTime(2000, 10, 10);
            var results = await client.GetTimesheetsByStatusAsync(date, date, "A");
            var content = Encoding.UTF8.GetString(handler[0].RequestContent);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Timesheet\" filter=\"newer-than,older-than\" field=\"starts,starts\" method=\"equal to\" limit=\"0,1000\"><Date><month>10</month><day>10</day><year>2000</year></Date><Date><month>10</month><day>10</day><year>2000</year></Date><Timesheet><status>A</status></Timesheet><_Return><status/><name /><total/><notes /><userid /><starts /></_Return></Read></request>", content);
        }

        [TestMethod]
        public async Task OpenAirClientGetCustomers_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Customer><id>1000</id><name>A</name></Customer><Customer><id>1001</id><name>B</name></Customer></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var date = new DateTime(2000, 10, 10);
            var results = await client.GetAllActiveCustomersAsync();
            var content = Encoding.UTF8.GetString(handler[0].RequestContent);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Customer\" method=\"equal to\" limit=\"0,1000\"><Customer><active>1</active></Customer><_Return><id/><name /></_Return></Read></request>", content);
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public async Task OpenAirClientGetAllActiveBookings_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Booking><userid>257</userid><booking_typeid>9</booking_typeid><id>95603</id><ownerid>265</ownerid><projectid>3410</projectid><customerid>10</customerid></Booking></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var bookings = await client.GetAllActiveBookingsAsync(new DateTime(2019, 1, 1));
            var content = Encoding.UTF8.GetString(handler[0].RequestContent);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Booking\" filter=\"newer-than,older-than\" field=\"enddate,startdate\" method=\"equal to\" limit=\"0,1000\"><Date><month>12</month><day>31</day><year>2018</year></Date><Date><month>1</month><day>2</day><year>2019</year></Date><Booking><approval_status>A</approval_status></Booking><_Return><id/><userid/><ownerid /><projectid /><customerid /><booking_typeid /></_Return></Read></request>", content);
            Assert.AreEqual(257, bookings[0].UserId);
            Assert.AreEqual(10, bookings[0].CustomerId);
        }

        [TestMethod]
        public async Task OpenAirClientGetUsersByActiveAsync_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<response><Auth status=\"0\"></Auth ><Read status=\"0\"><User></User></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var user = await client.GetAllUsersAsync();
            var content = Encoding.UTF8.GetString(handler[0].RequestContent);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"User\" method=\"all\" limit=\"0,1000\"><_Return><id /><name /><addr /><departmentid /><active /><line_managerid /><user_locationid /><usr_start_date__c/></_Return></Read></request>", content);
        }

        [TestMethod]
        public async Task OpenAirClientGetAllDepartments_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "U", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Department><userid>1</userid><name>.NET</name><id>101</id></Department></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var departments = await client.GetAllDepartmentsAsync();
            var content = Encoding.UTF8.GetString(handler[0].RequestContent);
            var department = departments.First();

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>U</user><password>P</password></Login></Auth><Read type=\"Department\" method=\"all\" limit=\"0,1000\"><_Return><id /><name /><userid /></_Return></Read></request>", content);
            Assert.AreEqual(101, department.Id);
            Assert.AreEqual(".NET", department.Name);
            Assert.AreEqual(1, department.UserId);
        }

        [TestMethod]
        public async Task OpenAirConnector_GetUnsubmittedTimesheetsAsync()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set(CreateTimesheetXmlContent((UserId: 3, new DateOnly(2019, 1, 28), Status: 'S', Hours: 40)))
                .Set(CreateTimesheetXmlContent(
                    (UserId: 2, new DateOnly(2019, 2, 28), Status: 'A', Hours: 30),
                    (UserId: 3, new DateOnly(2019, 2, 28), Status: 'S', Hours: 40)));

            var client = new OpenAirClient(() => handler, options);
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);
            var user = CreateUser(2, "Test", "Q", "d@e.f");
            var date = new DateTime(2019, 2, 1);

            storageService.GetAllActiveUsersAsync().ReturnsForAnyArgs(new[] { user });

            // Act
            var timesheets = await connector.GetUnsubmittedTimesheetsAsync(date, date, TimesheetStates.Unsubmitted, "d@e.f", true, string.Empty, null);

            Assert.AreEqual(1, timesheets.Count);
            Assert.AreEqual("Test", timesheets[0].UserName);
            Assert.AreEqual("Q", timesheets[0].DepartmentName);
            Assert.AreEqual(30, timesheets[0].Total);
        }

        [TestMethod]
        public async Task OpenAirConnector_GetUnsubmittedTimesheetsForEndOfMonthAsync()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set(CreateTimesheetXmlContent(
                    (UserId: 3, new DateOnly(2020, 03, 23), Status: 'S', Hours: 40)))
                .Set(CreateTimesheetXmlContent(
                    (UserId: 2, new DateOnly(2020, 03, 30), Status: 'A', Hours: 16),
                    (UserId: 1, new DateOnly(2020, 03, 30), Status: 'S', Hours: 40),
                    (UserId: 4, new DateOnly(2020, 03, 30), Status: 'A', Hours: 12),
                    (UserId: 3, new DateOnly(2020, 03, 30), Status: 'S', Hours: 8)));

            var client = new OpenAirClient(() => handler, options);
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);
            var user1 = CreateUser(1, "Test 1", "Q", "d@e.f");
            var user2 = CreateUser(2, "Test 2", "Q", "d@e.f");
            var user3 = CreateUser(3, "Test 3", "Q", "d@e.f");
            var user4 = CreateUser(4, "Test 4", "Q", "d@e.f");
            var date = new DateTime(2020, 3, 31);

            var user4HourValue = new Functions.Models.Domains.Plugins.PluginPropertyValue
            {
                Key = "Hour",
                Value = 30
            };

            user4.Properties = new Dictionary<string, Functions.Models.Domains.Plugins.PluginPropertyValue[][]>
            {
                { "OpenAir", new [] { new[] { user4HourValue } } }
            };

            storageService.GetAllActiveUsersAsync().ReturnsForAnyArgs(new[] { user1, user2, user3, user4 });

            // Act
            var timesheets = await connector.GetUnsubmittedTimesheetsAsync(date, date, TimesheetStates.Unsubmitted, "d@e.f", true, "Hour", null);

            Assert.AreEqual(1, timesheets.Count);
            Assert.AreEqual("Test 3", timesheets[0].UserName);
            Assert.AreEqual(8, timesheets[0].Total);
        }

        [TestMethod]
        public async Task OpenAirConnector_GetUnsubmittedTimesheetsShouldFilterNotStartedUsersAsync()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set(CreateTimesheetXmlContent(
                    (UserId: 1, new DateOnly(2022, 1, 28), Status: 'S', Hours: 40),
                    (UserId: 6, new DateOnly(2022, 1, 28), Status: 'S', Hours: 8)))
                .Set(CreateTimesheetXmlContent(
                    (UserId: 2, new DateOnly(2022, 1, 28), Status: 'A', Hours: 8),
                    (UserId: 3, new DateOnly(2022, 1, 28), Status: 'A', Hours: 16),
                    (UserId: 5, new DateOnly(2022, 1, 28), Status: 'A', Hours: 16)));

            var client = new OpenAirClient(() => handler, options);
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);
            var user1 = CreateUser(1, "Test 1", "Q", "d@e.f");
            var user2 = CreateUser(2, "Test 2", "Q", "d@e.f");
            var user3 = CreateUser(3, "Test 3", "Q", "d@e.f", startDate: new DateTime(2022, 1, 27));
            var user4 = CreateUser(4, "Test 4", "Q", "d@e.f", startDate: new DateTime(2022, 1, 31));
            var user5 = CreateUser(5, "Test 5", "Q", "d@e.f", startDate: new DateTime(2022, 1, 25));
            var user6 = CreateUser(6, "Test 6", "Q", "d@e.f", startDate: new DateTime(2022, 1, 27));
            var date = new DateTime(2022, 1, 28);

            user5.Properties = CreateUserHoursProperty(20);
            user6.Properties = CreateUserHoursProperty(40);
            storageService.GetAllActiveUsersAsync().ReturnsForAnyArgs(new[] { user1, user2, user3, user4, user5, user6 });

            // Act
            var timesheets = await connector.GetUnsubmittedTimesheetsAsync(date, date, TimesheetStates.Unsubmitted, "d@e.f", true, "Hour", null);

            Assert.AreEqual(2, timesheets.Count);
            Assert.AreEqual("Test 2", timesheets[0].UserName);
            Assert.AreEqual(40, timesheets[0].UtilizationInHours);
            Assert.AreEqual(8, timesheets[0].Total);
            Assert.AreEqual("Test 6", timesheets[1].UserName);
            Assert.AreEqual(16, timesheets[1].UtilizationInHours);
            Assert.AreEqual(8, timesheets[1].Total);
        }

        [TestMethod]
        public async Task OpenAirConnector_GetUnsubmittedTimesheetsWithOldDataAsync()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Timesheet><status>A</status><userid>397</userid><name>PTO</name><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>283</userid><name>PTO</name><total>8.00</total><starts><Date><month>03</month><day>04</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>520</userid><name>PTO</name><total>16.00</total><starts><Date><month>03</month><day>04</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>283</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>722</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>397</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>133</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>921</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>283</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>921</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>520</userid><name>02/25/19 to 03/03/19</name><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes></notes></Timesheet><Timesheet><status>A</status><userid>722</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet></Read ></response>")
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"></Read ></response>")
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);
            var date = new DateTime(2019, 3, 6);

            storageService
                .GetAllActiveUsersAsync()
                .ReturnsForAnyArgs(new[]
                {
                    CreateUser(397, "A", ".NET", "d@e.f"),
                    CreateUser(283, "B", ".NET", "d@e.f"),
                    CreateUser(520, "C", ".NET", "d@e.f"),
                    CreateUser(722, "D", ".NET", "d@e.f"),
                    CreateUser(133, "E", ".NET", "d@e.f"),
                    CreateUser(921, "F", ".NET", "d@e.f"),
                });

            // Act
            var timesheets = await connector.GetUnsubmittedTimesheetsAsync(date, date, TimesheetStates.Unsubmitted, "d@e.f", true, string.Empty, null);

            Assert.AreEqual(6, timesheets.Count);
        }

#pragma warning disable CS4014

        [TestMethod]
        public async Task OpenAirShouldSyncUsers()
        {
            var user1 = new OpenAirClient.User
            {
                Id = 1000,
                Name = "A",
                DepartmentId = 2000,
                Active = true,
                Address = new[]
                {
                    new OpenAirClient.Address { Email = "jhon.doe@mentormate.com" }
                },
                ManagerId = 1010
            };
            var user2 = new OpenAirClient.User
            {
                Id = 1010,
                Name = "B",
                DepartmentId = 2000,
                Active = true,
                Address = new[]
                {
                    new OpenAirClient.Address { Email = "bill.manager@mentormate.com" }
                }
            };
            var dep = new OpenAirClient.Department
            {
                Id = 2000,
                Name = "QA",
                UserId = 1010
            };
            var customer = new OpenAirClient.Customer
            {
                Id = 3000,
                Name = "MM"
            };
            var booking = new OpenAirClient.Booking
            {
                Id = 4000,
                UserId = 1000,
                CustomerId = 3000,
                ProjectId = 6000,
                OwnerId = 1010
            };
            var client = Substitute.For<IOpenAirClient>();

            client.GetAllUsersAsync().Returns(new[] { user1, user2 });
            client.GetAllDepartmentsAsync().Returns(new[] { dep });
            client.GetAllActiveCustomersAsync().Returns(new[] { customer });
            client.GetAllActiveBookingsAsync(DateTime.MinValue).ReturnsForAnyArgs(new[] { booking });

            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);

            storageService
                .GetAllUsersAsync()
                .ReturnsForAnyArgs(new[]
                {
                    CreateUser(1000, "A", ".NET", "bill.manager@mentormate.com", 1),
                    CreateUser(1011, "B", ".NET", "bill.manager@mentormate.com", 2)
                });

            // Act
            await connector.SyncUsersAsync();

            storageService
                .Received()
                .UpdateUsersAsync(Arg.Is<IReadOnlyList<User>>(it =>
                    it.Count == 1
                    && it.First().Manager.Email == "bill.manager@mentormate.com"
                    && it.First().Customers.First().Name == "MM"
                    && it.First().Department.Name == "QA"
                    && it.First().Department.Owner.Email == "bill.manager@mentormate.com"));
        }

        [TestMethod]
        public async Task OpenAirShouldSyncUsersAndUpdateDepartmentName()
        {
            var client = Substitute.For<IOpenAirClient>();
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var user = new OpenAirClient.User
            {
                Id = 1000,
                Name = "A",
                DepartmentId = 2000,
                Active = true,
                Address = new[]
                {
                    new OpenAirClient.Address { Email = "jhon.doe@mentormate.com" }
                },
                ManagerId = 1010
            };
            var userManager = new OpenAirClient.User
            {
                Id = 1010,
                Name = "B",
                DepartmentId = 2000,
                Active = true,
                Address = new[]
                {
                    new OpenAirClient.Address { Email = "bill.manager@mentormate.com" }
                }
            };

            var dep = new OpenAirClient.Department
            {
                Id = 2000,
                Name = "Test",
                UserId = 1010
            };

            client.GetAllUsersAsync().Returns(new[] { user, userManager });
            client.GetAllDepartmentsAsync().Returns(new[] { dep });
            client.GetAllActiveCustomersAsync().Returns(new OpenAirClient.Customer[0]);
            client.GetAllActiveBookingsAsync(DateTime.MinValue).ReturnsForAnyArgs(new OpenAirClient.Booking[0]);
            storageService
                .GetAllUsersAsync()
                .ReturnsForAnyArgs(new[]
                {
                    CreateUser(1000, "A", "OldDep", "bill.manager@mentormate.com", 2000, 1010, 1010),
                    CreateUser(1010, "B", "Test", null, 2000, 0, 1010)
                });

            // Act
            await connector.SyncUsersAsync();

            storageService.Received().UpdateUsersAsync(Arg.Is<IReadOnlyList<User>>(list =>
                list.Any(u => u.OpenAirUserId == 1000 && u.Department.Name == "Test")));
        }

        [TestMethod]
        public async Task OpenAirShouldSyncUsersAndReturnNamesToLower()
        {
            var user1 = new OpenAirClient.User
            {
                Id = 1000,
                Name = "A",
                DepartmentId = 2000,
                Active = true,
                Address = new[]
                 {
                    new OpenAirClient.Address { Email = "jhon.dOe@mentormate.com" }
                },
                ManagerId = 1010
            };
            var user2 = new OpenAirClient.User
            {
                Id = 1010,
                Name = "B",
                DepartmentId = 2000,
                Active = true,
                Address = new[]
                {
                    new OpenAirClient.Address { Email = "bill.Manager@mentormate.com" }
                }
            };
            var dep = new OpenAirClient.Department
            {
                Id = 2000,
                Name = "QA",
                UserId = 1010
            };
            var customer = new OpenAirClient.Customer
            {
                Id = 3000,
                Name = "MM"
            };
            var booking = new OpenAirClient.Booking
            {
                Id = 4000,
                UserId = 1000,
                CustomerId = 3000,
                ProjectId = 6000,
                OwnerId = 1010
            };
            var client = Substitute.For<IOpenAirClient>();

            client.GetAllUsersAsync().Returns(new[] { user1, user2 });
            client.GetAllDepartmentsAsync().Returns(new[] { dep });
            client.GetAllActiveCustomersAsync().Returns(new[] { customer });
            client.GetAllActiveBookingsAsync(DateTime.MinValue).ReturnsForAnyArgs(new[] { booking });

            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);

            storageService
                .GetAllUsersAsync()
                .ReturnsForAnyArgs(new[]
                {
                    CreateUser(1000, "A", ".NET", "bill.manager@mentormate.com", 1),
                    CreateUser(1011, "B", ".NET", "bill.manager@mentormate.com", 2)
                });

            // Act
            await connector.SyncUsersAsync();

            storageService
                .Received()
                .UpdateUsersAsync(Arg.Is<IReadOnlyList<User>>(it =>
                it.All(user => user.Email == user.Email.ToLower())
                && it.All(user => user.Name == user.Name.ToLower())));
        }

#pragma warning restore CS4014

        private static Dictionary<string, Functions.Models.Domains.Plugins.PluginPropertyValue[][]> CreateUserHoursProperty(int hours)
        {
            var hourValue = new Functions.Models.Domains.Plugins.PluginPropertyValue
            {
                Key = "Hour",
                Value = hours
            };

            return new Dictionary<string, Functions.Models.Domains.Plugins.PluginPropertyValue[][]>
            {
                { "OpenAir", new [] { new[] { hourValue } } }
            };
        }

        private static string CreateXmlResponse<T>(Func<T, string> selector, params T[] args) =>
            $"<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth><Read status=\"0\">{string.Join(string.Empty, args.Select(selector))}</Read></response>";

        private static string CreateTimesheetXmlContent(params (int UserId, DateOnly Date, char Status, int Hours)[] args) =>
            CreateXmlResponse(CreateTimesheetXml, args);

        private static string CreateTimesheetXml((int UserId, DateOnly Date, char Status, int Hours) data) =>
            $"<Timesheet><status>{data.Status}</status><userid>{data.UserId}</userid><name></name><total>{data.Hours}</total>" +
            $"<starts><Date><month>{data.Date.Month}</month><day>{data.Date.Day}</day><year>{data.Date.Year}</year></Date></starts>" +
            "<notes></notes></Timesheet>";

        private static User CreateUser(
            long id,
            string name,
            string departmentName,
            string managerEmail,
            long departmentId = 1,
            long managerId = 100,
            long? departmentOwner = null,
            DateTime? startDate = null) =>
            new User
            {
                Name = name,
                Department = new Department
                {
                    Name = departmentName,
                    OpenAirDepartmentId = departmentId,
                    Owner = departmentOwner.HasValue ? new UserReference { OpenAirUserId = departmentOwner.Value } : null,
                },
                Manager = string.IsNullOrEmpty(managerEmail) ? null : new UserReference { Email = managerEmail, OpenAirUserId = managerId },
                OpenAirUserId = id,
                Active = true,
                StartDate = startDate,
            };
    }
}
