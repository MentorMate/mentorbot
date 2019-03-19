using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.OpenAir;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
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

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Timesheet\" filter=\"newer-than,older-than\" field=\"starts,starts\" method=\"all\" limit=\"1000\"><Date><month>10</month><day>10</day><year>2000</year></Date><Date><month>10</month><day>10</day><year>2000</year></Date><_Return><status/><name /><total/><notes /><userid /><starts /></_Return></Read></request>", content);
            Assert.AreEqual(1, results.Length);

            var first = results.First();
            Assert.AreEqual(722, first.UserId);
            Assert.AreEqual(2, first.StartDate.Date.Month);
            Assert.AreEqual(25, first.StartDate.Date.Day);
            Assert.AreEqual(2019, first.StartDate.Date.Year);
        }

        [TestMethod]
        public async Task OpenAirClientGetCustomers_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Customer><id>1000</id><name>A</name></Customer><Customer><id>1001</id><name>B</name></Customer></Read ></response>");
            
            var client = new OpenAirClient(() => handler, options);
            var date = new DateTime(2000, 10, 10);
            var results = await client.GetAllActiveCustomersByCreaetedDateAsync(date, null);
            var content = Encoding.UTF8.GetString(handler[0].RequestContent);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Customer\" filter=\"newer-than\" field=\"createtime\" method=\"equal to\" limit=\"1000\"><Date><month>10</month><day>10</day><year>2000</year></Date><Customer><active>1</active></Customer><_Return><id/><name /></_Return></Read></request>", content);
            Assert.AreEqual(2, results.Length);
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

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Booking\" filter=\"newer-than,older-than\" field=\"enddate,startdate\" method=\"equal to\" limit=\"1000\"><Date><month>1</month><day>1</day><year>2019</year></Date><Date><month>1</month><day>1</day><year>2019</year></Date><Booking><approval_status>A</approval_status></Booking><_Return><id/><userid/><ownerid /><projectid /><customerid /><booking_typeid /></_Return></Read></request>", content);
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
            var user = await client.GetUsersByActiveAsync(true);
            var content = Encoding.UTF8.GetString(handler[0].RequestContent);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"User\" method=\"equal to\" limit=\"1000\"><User><active>1</active></User><_Return><id /><name /><addr /><departmentid /><active /><line_managerid /><user_locationid /></_Return></Read></request>", content);
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

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>U</user><password>P</password></Login></Auth><Read type=\"Department\" method=\"all\" limit=\"1000\"><_Return><id /><name /><userid /></_Return></Read></request>", content);
            Assert.AreEqual(101, department.Id);
            Assert.AreEqual(".NET", department.Name);
            Assert.AreEqual(1, department.UserId);
        }

        [TestMethod]
        public async Task OpenAirConnector_GetUnsubmittedTimesheetsAsync()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth><Read status=\"0\"><Timesheet><status>S</status><userid>3</userid><name></name><total>40.00</total><starts><Date><month>01</month><day>28</day><year>2019</year></Date></starts><notes></notes></Timesheet></Read ></response>")
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth><Read status=\"0\"><Timesheet><status>A</status><userid>2</userid><name>A</name><total>30.00</total><starts><Date><month>02</month><day>28</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);
            var user = CreateUser(2, "Test", "Q");
            var date = new DateTime(2019, 2, 1);

            storageService.GetUsersByIdList(null).ReturnsForAnyArgs(new[] { user });

            // Act
            var timesheets = await connector.GetUnsubmittedTimesheetsAsync(date, TimesheetStates.Unsubmitted, null);

            Assert.AreEqual(1, timesheets.Count);
            Assert.AreEqual("Test", timesheets[0].UserName);
            Assert.AreEqual("A", timesheets[0].Name);
            Assert.AreEqual("Q", timesheets[0].DepartmentName);
            Assert.AreEqual(30, timesheets[0].Total);
        }

        [TestMethod]
        public async Task OpenAirConnector_GetUnsubmittedTimesheetsWithOldDataAsync()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler()
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Timesheet><status>A</status><userid>397</userid><name>PTO</name><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>283</userid><name>PTO</name><total>8.00</total><starts><Date><month>03</month><day>04</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>520</userid><name>PTO</name><total>16.00</total><starts><Date><month>03</month><day>04</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>283</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>722</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>397</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>133</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>921</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>283</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>921</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>520</userid><name>02/25/19 to 03/03/19</name><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes></notes></Timesheet><Timesheet><status>A</status><userid>722</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet></Read ></response>")
                .Set("<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"></Read ></response>");

            var client = new OpenAirClient(() => handler, options);
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);
            var date = new DateTime(2019, 3, 6);

            storageService
                .GetUsersByIdList(null)
                .ReturnsForAnyArgs(new[]
                {
                    CreateUser(397, "A", ".NET"),
                    CreateUser(283, "B", ".NET"),
                    CreateUser(520, "C", ".NET"),
                    CreateUser(722, "D", ".NET"),
                    CreateUser(133, "E", ".NET"),
                    CreateUser(921, "F", ".NET"),
                });

            // Act
            var timesheets = await connector.GetUnsubmittedTimesheetsAsync(date, TimesheetStates.Unsubmitted, null);

            Assert.AreEqual(6, timesheets.Count);
        }

        #pragma warning disable CS4014

        [TestMethod]
        public async Task OpenAirConnector_SyncUsers()
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
            var cust = new OpenAirClient.Customer
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
            client.GetAllActiveCustomersAsync().Returns(new[] { cust });
            client.GetAllActiveBookingsAsync(DateTime.MinValue).ReturnsForAnyArgs(new[] { booking });

            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);

            storageService
                .GetAllUsers()
                .ReturnsForAnyArgs(new[]
                {
                    CreateUser(1000, "A", ".NET", 1),
                    CreateUser(1011, "B", ".NET", 2)
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

        #pragma warning restore CS4014

        private static User CreateUser(long id, string name, string departmentName, long departmentId = 1) =>
            new User { Name = name, Department = new Department { Name = departmentName, OpenAirDepartmentId = departmentId }, OpenAirUserId = id, Active = true };
    }
}
