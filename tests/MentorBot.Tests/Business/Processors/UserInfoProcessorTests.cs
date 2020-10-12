using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Base;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;
using MentorBot.Functions.Processors.UserInfo;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Processors
{
    [TestClass]
    [TestCategory("Business.Processors")]
    public sealed class UserInfoProcessorTests
    {
        private UserInfoProcessor _processor;
        private IStorageService _storageService;

        [TestInitialize]
        public void TestInitialize()
        {
            _storageService = Substitute.For<IStorageService>();
            _processor = new UserInfoProcessor(_storageService);
        }

        [TestMethod]
        public void UserInfoProcessorSubjectShoudBeUser()
        {
            Assert.AreEqual(_processor.Subject, "User");
        }

        [TestMethod]
        public void NameShouldBeOk()
        {
            Assert.AreEqual(_processor.Name, "MentorBot.Functions.Processors.UserInfo.UserInfoProcessor");
        }

        [TestMethod]
        public async Task UserInfoShoudReturnInfoIfNoName()
        {
            var result = await _processor.ProcessCommandAsync(GetInfo(null), CreateEvent(null), null, null);
            Assert.AreEqual(result.Text, "User was not found!");
        }

        [TestMethod]
        public async Task UserInfoShoudReturnInfoIfNotFound()
        {
            var user = new User { Id = "A", Name = "Doe, Jhon", Email = "jhon.doe@abc.de", OpenAirUserId = 1 };
            var users = new[] { user };
            _storageService.GetAllUsersAsync().Returns(users);

            var result = await _processor.ProcessCommandAsync(GetInfo("Jhon Mark"), CreateEvent("jhon.doe@abc.de"), null, null);

            Assert.AreEqual("User with name Jhon Mark was not found!", result.Text);
        }

        [TestMethod]
        public async Task UserInfoShoudReturnInfoIfFound()
        {
            var user1 = GetUser("Doe, Jhon", "jhon.doe@abc.de", "A", 1, "Managers", null, null);
            var user2 = GetUser("Cherry, Merry", "merry.cherry@abc.de", "B", 2, ".NET", 1, "jhon.doe@abc.de", "First", "Second");
            var users = new[] { user1, user2 };
            _storageService.GetAllUsersAsync().Returns(users);

            var result = await _processor.ProcessCommandAsync(GetInfo("Merry Cherry"), CreateEvent("jhon.doe@abc.de"), null, null);
            var body = result.Cards[0].Sections[0].Widgets[0].KeyValue;

            Assert.AreEqual("Cherry, Merry", body.TopLabel);
            Assert.AreEqual("First, Second", body.BottomLabel);
        }

        [TestMethod]
        public async Task UserInfoShoudNotReturnIfNotRequestedMyManager()
        {
            var user1 = GetUser("Doe, Jhon", "jhon.doe@abc.de", "A", 1, "Managers", null, null);
            var user2 = GetUser("Cherry, Merry", "merry.cherry@abc.de", "B", 2, ".NET", 1, "jhon.doe@abc.de", "First", "Second");
            var users = new[] { user1, user2 };
            _storageService.GetAllUsersAsync().Returns(users);

            var result = await _processor.ProcessCommandAsync(GetInfo("Merry Cherry"), CreateEvent("david.eldon@abc.de"), null, null);

            Assert.AreEqual("User with name Merry Cherry was not found!", result.Text, false, CultureInfo.InvariantCulture);
        }

        private static TextDeconstructionInformation GetInfo(string name) =>
            new TextDeconstructionInformation(
                "Get user info for " + name,
                "User",
                SentenceTypes.Command,
                new Dictionary<string, string[]> { { "Text", name == null ? null :  new [] { name } } },
                null,
                1);

        private static ChatEvent CreateEvent(string senderEmail)
        {
            var sender = new ChatEventMessageSender() { Email = senderEmail };
            var space = new ChatEventSpace();
            var message = new ChatEventMessage { Sender = sender };
            return new ChatEvent { Space = space, Message = message };
        }

        private static User GetUser(string name, string email, string id, long oaId, string departmentName, long? managerId, string managerEmail, params string[] cuctomers) =>
            new User
            {
                Id = id,
                Name = name,
                Email = email,
                OpenAirUserId = oaId,
                Department = new Department
                {
                    Name = departmentName
                },
                Manager = managerId.HasValue ? new UserReference { OpenAirUserId = managerId.Value, Email = managerEmail } : null,
                Customers = cuctomers.Select(it => new Customer { Name = it }).ToArray(),
            };
    }
}