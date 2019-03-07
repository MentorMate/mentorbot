using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.OpenAir;
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
        public async Task OpenAirClientGetTimesheets_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler
            {
                ResponseContent = "<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Timesheet><status>A</status><userid>722</userid><name>02/25/19 to 03/03/19 PTO</name><total>8.00</total><starts><Date><hour/><minute/><timezone/><second/><month>02</month><day>25</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet></Read ></response>"
            };

            var client = new OpenAirClient(() => handler, options);
            var date = new DateTime(2000, 10, 10);
            var results = await client.GetTimesheetsAsync(date, date);
            var content = Encoding.UTF8.GetString(handler.Content);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Timesheet\" filter=\"newer-than,older-than\" field=\"starts,starts\" method=\"all\" limit=\"1000\"><Date><month>10</month><day>10</day><year>2000</year></Date><Date><month>10</month><day>10</day><year>2000</year></Date><_Return><status/><name /><total/><notes /><userid /><starts /></_Return></Read></request>", content);
            Assert.AreEqual(1, results.Length);

            var first = results.First();
            Assert.AreEqual(722, first.UserId);
            Assert.AreEqual(2, first.StartDate.Date.Month);
            Assert.AreEqual(25, first.StartDate.Date.Day);
            Assert.AreEqual(2019, first.StartDate.Date.Year);
        }

        [TestMethod]
        public async Task OpenAirClientGetUserByIdAsync_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "R", "P");
            var handler = new MockHttpMessageHandler
            {
                ResponseContent = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><User><departmentid>2</departmentid><timezone>+0200</timezone><name>Doe, Jhon</name><id>100</id><addr><Address><salutation/><mobile/><state/><email>jhon.doe@mentormate.com</email><addr2/><city>City</city><fax/><contact_id/><addr1/><id>-1</id><middle/><country/><first>Jhon</first><last>Doe</last><phone/><addr4/><zip/><addr3/></Address></addr></User></Read ></response>"
            };

            var client = new OpenAirClient(() => handler, options);
            var user = await client.GetUserByIdAsync(100);
            var content = Encoding.UTF8.GetString(handler.Content);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"User\" method=\"equal to\" limit=\"1\"><User><id>100</id></User><_Return><id /><name /><timezone/><addr /><departmentid /></_Return></Read></request>", content);
            Assert.AreEqual(100, user.Id);
            Assert.AreEqual("Doe, Jhon", user.Name);
            Assert.AreEqual(2, user.DepartmentId);
            Assert.AreEqual("jhon.doe@mentormate.com", user.Address.First().Email);
        }

        [TestMethod]
        public async Task OpenAirClientGetAllDepartments_ShouldParseResult()
        {
            var options = new OpenAirOptions("http://localhost/", "MM", "K", "U", "P");
            var handler = new MockHttpMessageHandler
            {
                ResponseContent = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Department><userid>1</userid><name>.NET</name><id>101</id></Department></Read ></response>"
            };

            var client = new OpenAirClient(() => handler, options);
            var departments = await client.GetAllDepartmentsAsync();
            var content = Encoding.UTF8.GetString(handler.Content);
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
            var handler = new MockHttpMessageHandler
            {
                ResponseContent = "<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth><Read status=\"0\"><Timesheet><status>A</status><userid>2</userid><name>A</name><total>30.00</total><starts><Date><month>02</month><day>28</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>S</status><userid>3</userid><name></name><total>40.00</total><starts><Date><month>01</month><day>28</day><year>2019</year></Date></starts><notes></notes></Timesheet></Read ></response>"
            };

            var client = new OpenAirClient(() => handler, options);
            var storageService = Substitute.For<IStorageService>();
            var connector = new OpenAirConnector(client, storageService);
            var user = new User { Name = "Test", Department = new Department { Name = "Q" }, OpenAirUserId = 2 };
            var date = new DateTime(2019, 2, 1);

            storageService.GetUsersByIdList(null).ReturnsForAnyArgs(new[] { user });

            // Act
            var timesheets = await connector.GetUnsubmittedTimesheetsAsync(date, null);

            var content = Encoding.UTF8.GetString(handler.Content);

            Assert.AreEqual("<request API_version=\"1.0\" client=\"MM\" client_ver=\"1.0\" namespace=\"default\" key=\"K\"><Auth><Login><company>MM</company><user>R</user><password>P</password></Login></Auth><Read type=\"Timesheet\" filter=\"newer-than,older-than\" field=\"starts,starts\" method=\"all\" limit=\"1000\"><Date><month>1</month><day>20</day><year>2019</year></Date><Date><month>2</month><day>2</day><year>2019</year></Date><_Return><status/><name /><total/><notes /><userid /><starts /></_Return></Read></request>", content);
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
            var handler = new MockHttpMessageHandler
            {
                ResponseContent = "<?xml version=\"1.0\" standalone=\"yes\"?><response><Auth status=\"0\"></Auth ><Read status=\"0\"><Timesheet><status>A</status><userid>397</userid><name>PTO</name><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>283</userid><name>PTO</name><total>8.00</total><starts><Date><month>03</month><day>04</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>520</userid><name>PTO</name><total>16.00</total><starts><Date><month>03</month><day>04</day><year>2019</year></Date></starts><notes>PTO</notes></Timesheet><Timesheet><status>A</status><userid>283</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>722</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>397</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>133</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>921</userid><name/><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>283</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>921</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet><Timesheet><status>A</status><userid>520</userid><name>02/25/19 to 03/03/19</name><total>32.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes></notes></Timesheet><Timesheet><status>A</status><userid>722</userid><name/><total>8.00</total><starts><Date><month>02</month><day>25</day><year>2019</year></Date></starts><notes/></Timesheet></Read ></response>"
            };

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
            var timesheets = await connector.GetUnsubmittedTimesheetsAsync(date, null);

            Assert.AreEqual(6, timesheets.Count);
        }

        private static User CreateUser(long id, string name, string departmentName) =>
            new User { Name = name, Department = new Department { Name = departmentName }, OpenAirUserId = id };
    }
}
