using CoreHelpers.WindowsAzure.Storage.Table;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Settings;
using MentorBot.Functions.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MentorBot.Tests.Business.Services
{
    [TestClass]
    [TestCategory("Business.Services")]
    public sealed class TableStorageServiceTests
    {
        private TableStorageService _serviceConnected;
        private TableStorageService _serviceNotConnected;

        [TestInitialize]
        public void TestInitialize()
        {
            _serviceConnected = new TableStorageService(new Functions.Models.Options.AzureCloudOptions("connection string", string.Empty, string.Empty, string.Empty));
            _serviceNotConnected = new TableStorageService(null);
        }

        #region NotConnected tests

        [TestMethod]
        public async Task AddUsersAsync_NotConnected_returns_False()
        {
            var result = await _serviceNotConnected.AddUsersAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UpdateUsersAsync_NotConnected_returns_False()
        {
            var result = await _serviceNotConnected.UpdateUsersAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_NotConnected_returns_EmptyList()
        {
            var result = await _serviceNotConnected.GetAllUsersAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<User>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetUsersByIdListAsync_NotConnected_returns_EmptyList()
        {
            var result = await _serviceNotConnected.GetUsersByIdListAsync(null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<User>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task AddAddressesAsync_NotConnected_returns_False()
        {
            var result = await _serviceNotConnected.AddAddressesAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetAddressesAsync_NotConnected_returns_EmptyList()
        {
            var result = await _serviceNotConnected.GetAddressesAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<GoogleAddress>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetMessagesAsync_NotConnected_returns_EmptyList()
        {
            var result = await _serviceNotConnected.GetMessagesAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IReadOnlyList<Message>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task SaveMessageAsync_NotConnected_returns_False()
        {
            var result = await _serviceNotConnected.SaveMessageAsync(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetSettingsAsync_NotConnected_returns_EmptySettings()
        {
            var result = await _serviceNotConnected.GetSettingsAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(MentorBotSettings));
            Assert.AreEqual(nameof(MentorBotSettings), result.Key);
            Assert.AreEqual(0, result.Processors.Count);
        }

        [TestMethod]
        public async Task SaveSettingsAsync_NotConnected_returns_False()
        {
            var result = await _serviceNotConnected.SaveSettingsAsync(null);

            Assert.IsFalse(result);
        }

        #endregion
    }
}
