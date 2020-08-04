using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services
{
    [TestClass]
    [TestCategory("Business.Services")]
    public sealed class TableStorageServiceTests
    {
        private TableStorageService _storageService;
        private ITableClientService _tableClientService;
        private List<User> _users;
        private List<GoogleAddress> _addresses;
        private List<Message> _messages;
        private List<Plugin> _plugins;

        [TestInitialize]
        public void TestInitialize()
        {
            _tableClientService = Substitute.For<ITableClientService>();

            _storageService = new TableStorageService(_tableClientService);
            _tableClientService.IsConnected.Returns(true);

            _users = new List<User>
            {
                new User { Id = "3C039142-CF1A-42CC-87E8-893D8791D4A9", OpenAirUserId = 1 },
                new User { Id = "C32EC69C-977C-4C42-B84E-13E51195FE6D", OpenAirUserId = 2 },
                new User { Id = "7408FCFA-6F55-4824-A192-A88D7C12FECE", OpenAirUserId = 3 },
            };

            _addresses = new List<GoogleAddress>
            {
                new GoogleAddress{ Id = "01BE6A6F-821B-465B-B2D9-7621C0A1C55B", PartitionKey = "5DF2E025-E886-43CE-A389-6A0DB9B74083", SpaceName = "Space 1", UserName = "User 1" },
                new GoogleAddress{ Id = "C5BAA1D5-7C96-4899-B15A-B846D98975D9", PartitionKey = "5DF2E025-E886-43CE-A389-6A0DB9B74083", SpaceName = "Space 1", UserName = "User 2" },
                new GoogleAddress{ Id = "F25E143F-67CD-40D4-A311-3A224E48A40C", PartitionKey = "5DF2E025-E886-43CE-A389-6A0DB9B74083", SpaceName = "Space 1", UserName = "User 3" }
            };

            _messages = new List<Message>
            {
                new Message{ Id = "01BE6A6F-821B-465B-B2D9-7621C0A1C55B", PartitionKey = "5DF2E025-E886-43CE-A389-6A0DB9B74083", Input = "question 1", Output = new ChatEventResult("text 1"), ProbabilityPercentage = 65 },
                new Message{ Id = "C5BAA1D5-7C96-4899-B15A-B846D98975D9", PartitionKey = "5DF2E025-E886-43CE-A389-6A0DB9B74083", Input = "question 2", Output = new ChatEventResult("text 2"), ProbabilityPercentage = 76 },
                new Message{ Id = "F25E143F-67CD-40D4-A311-3A224E48A40C", PartitionKey = "5DF2E025-E886-43CE-A389-6A0DB9B74083", Input = "question 3", Output = new ChatEventResult("text 3"), ProbabilityPercentage = 98 }
            };

            _plugins = new List<Plugin>
            {
                new Plugin { Name = "Processor 1", Enabled = true},
                new Plugin { Name = "Processor 2", Enabled = false},
            };
        }

        #region NotConnected tests

        [TestMethod]
        public async Task AddUsersAsync_NotConnected_returns_False()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.AddUsersAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UpdateUsersAsync_NotConnected_returns_False()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.UpdateUsersAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_NotConnected_returns_EmptyList()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.GetAllUsersAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<User>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task AddAddressesAsync_NotConnected_returns_False()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.AddAddressesAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetAddressesAsync_NotConnected_returns_EmptyList()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.GetAddressesAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<GoogleAddress>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetMessagesAsync_NotConnected_returns_EmptyList()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.GetMessagesAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<Message>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task SaveMessageAsync_NotConnected_returns_False()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.SaveMessageAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task AddOrUpdateUserAsync_NotConnected()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.AddOrUpdateUserAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetAllPluginsAsync_NotConnected_returns_EmptySettings()
        {
            ServiceLocator.EnsureServiceProvider();

            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.GetAllPluginsAsync();


            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<Plugin>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task AddOrUpdatePluginsAsync_NotConnected_returns_False()
        {
            _tableClientService.IsConnected.Returns(false);

            var result = await _storageService.AddOrUpdatePluginsAsync(null);

            Assert.IsFalse(result);
        }

        #endregion

        #region Connected tests

        [TestMethod]
        public async Task AddUsersAsync_Connected_stores_3_users()
        {
            var users = new List<User>();

            _tableClientService.MergeOrInsertAsync<User>(Arg.Do<User>(u => users.Add(u)));

            var result = await _storageService.AddUsersAsync(_users);

            Assert.AreEqual(3, users.Count);
            Assert.AreEqual(1, users[0].OpenAirUserId);
            Assert.AreEqual(2, users[1].OpenAirUserId);
            Assert.AreEqual(3, users[2].OpenAirUserId);

        }

        [TestMethod]
        public async Task UpdateUsersAsync_Connected_returns_true_and_updates_names_of_users()
        {
            _tableClientService.MergeAsync<User>(Arg.Do<IEnumerable<User>>(input =>
            {
                foreach (var u in input)
                {
                    var ind = _users.IndexOf(_users.FirstOrDefault(us => us.OpenAirUserId == u.OpenAirUserId));
                    _users[ind] = u;
                }
            }));

            var result = await _storageService.UpdateUsersAsync(new List<User>
            {
                new User { Id = "3C039142-CF1A-42CC-87E8-893D8791D4A9", OpenAirUserId = 1, Name = "User 1" },
                new User { Id = "7408FCFA-6F55-4824-A192-A88D7C12FECE", OpenAirUserId = 3, Name = "User 3" },
            });

            Assert.IsTrue(result);
            Assert.AreEqual("User 1", _users[0].Name);
            Assert.AreEqual("User 3", _users[2].Name);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_Connected_returns_List_of_3_users()
        {
            _tableClientService.QueryAsync<User>(Arg.Any<int>()).Returns(_users.AsQueryable());

            var result = await _storageService.GetAllUsersAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<User>));
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(3, result[2].OpenAirUserId);
        }

        [TestMethod]
        public async Task GetAllActiveUsersShouldReturnFromContext()
        {
            var user = new User { Id = "AA", Active = false };
            var user2 = new User { Id = "BB", Active = true };

            _tableClientService.QueryAsync<User>(2000).Returns(new[] { user, user2 }.AsQueryable());

            var result = await _storageService.GetAllActiveUsersAsync();

            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Id, "BB");
        }

        [TestMethod]
        public async Task GetUserByEmailShouldReturnFromContext()
        {
            var user = new User { Id = "AA" };

            _tableClientService.QueryAsync<User>("Email eq 'aa@bb.cc'", 1).Returns(new[] { user }.AsQueryable());

            var result = await _storageService.GetUserByEmailAsync("aa@bb.cc");

            Assert.AreEqual(result.Id, "AA");
        }

        [TestMethod]
        public async Task AddAddressesAsync_Connected_returns_true_and_stores_3_addresses()
        {
            var addresses = new List<GoogleAddress>();

            _tableClientService.MergeOrInsertAsync<GoogleAddress>(Arg.Do<GoogleAddress>(a => addresses.Add(a)));

            var result = await _storageService.AddAddressesAsync(_addresses);

            Assert.IsTrue(result);
            Assert.AreEqual(3, addresses.Count);
        }

        [TestMethod]
        public async Task AddOrUpdateUserAsyncShouldMergeOrInsert()
        {
            User user = null;

            _tableClientService.MergeOrInsertAsync(Arg.Do<User>(a => user = a));

            var result = await _storageService.AddOrUpdateUserAsync(_users[0]);

            Assert.AreEqual("3C039142-CF1A-42CC-87E8-893D8791D4A9", user?.Id);
        }

        [TestMethod]
        public async Task GetAddressesAsync_Connected_returns_a_list_of_3_addresses()
        {
            _tableClientService.QueryAsync<GoogleAddress>(Arg.Any<int>()).Returns(_addresses.AsQueryable());

            var result = await _storageService.GetAddressesAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<GoogleAddress>));
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetMessagesAsync_Connected_returns_a_list_of_3_messages()
        {
            _tableClientService.QueryAsync<Message>(Arg.Any<int>()).Returns(_messages.AsQueryable());

            var result = await _storageService.GetMessagesAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<Message>));
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task SaveMessageAsync_Connected_returns_true_and_stores_1_message()
        {
            var messages = new List<Message>();

            _tableClientService.MergeOrInsertAsync<Message>(Arg.Do<Message>(m => messages.Add(m)));

            var result = await _storageService.SaveMessageAsync(_messages[0]);

            Assert.IsTrue(result);
            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual("question 1", messages[0].PartitionKey);
            Assert.AreEqual("question 1", messages[0].Input);

        }

        [TestMethod]
        public async Task GetAllPluginsAsync_Connected_returns_correct_settings()
        {
            ServiceLocator.EnsureServiceProvider();

            _tableClientService.QueryAsync<Plugin>(1000).Returns(_plugins.AsQueryable());

            var result = await _storageService.GetAllPluginsAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Processor 2", result[1].Name);
            Assert.IsFalse(result[1].Enabled);
        }

        [TestMethod]
        public async Task AddOrUpdatePluginsAsync_Connected_returns_true()
        {
            var plugins = new List<Plugin>();

            _tableClientService.MergeOrInsertListAsync(Arg.Do<IEnumerable<Plugin>>(s => plugins.AddRange(s)));

            var result = await _storageService.AddOrUpdatePluginsAsync(_plugins);

            Assert.AreEqual(2, plugins.Count);
            Assert.AreEqual("Processor 1", plugins[0].Name);
            Assert.IsTrue(plugins[0].Enabled);
        }

        [TestMethod]
        public async Task StorageService_GetStatistics()
        {
            var data = new Statistics<TimesheetStatistics> { Id = "AA" };

            _tableClientService.QueryAsync<Statistics<TimesheetStatistics>>("Date eq '2020-10-01' AND Time eq '20:00'", 1000).Returns(new[] { data }.AsQueryable());

            var result = await _storageService.GetStatisticsAsync<TimesheetStatistics>("2020-10-01", "20:00");

            Assert.AreEqual(result.First().Id, "AA");
        }

        #endregion
    }
}
