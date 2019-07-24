using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreHelpers.WindowsAzure.Storage.Table.Models;
using MentorBot.Functions.Services.AzureStorage;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services.AzureStorage
{
    [TestClass]
    [TestCategory("Business.Services.Azure")]
    public sealed class TableClientServiceTests
    {
        [TestMethod]
        public async Task TableClientService_ShouldMergeRecords()
        {
            var client = Substitute.For<IAzureStorageContext>();
            var service = new TableClientService(client);
            var data = new[] { new Test() };

            await service.MergeAsync<Test>(data);

            Assert.IsTrue(service.IsConnected);
            client.Received().AddAttributeMapper(typeof(Test));
            await client.Received().CreateTableAsync(typeof(Test), true);
            await client.Received().MergeAsync<Test>(data);
        }

        [TestMethod]
        public async Task TableClientService_ShouldMergeOrInsertRecord()
        {
            var client = Substitute.For<IAzureStorageContext>();
            var service = new TableClientService(client);
            var data = new Test();

            await service.MergeOrInsertAsync<Test>(data);

            Assert.IsTrue(service.IsConnected);
            client.Received().AddAttributeMapper(typeof(Test));
            await client.Received().CreateTableAsync(typeof(Test), true);
            await client.Received().MergeOrInsertAsync<Test>(data);
        }

        [TestMethod]
        public async Task TableClientService_ShouldQuery()
        {
            var client = Substitute.For<IAzureStorageContext>();
            var service = new TableClientService(client);
            var data = new Test();

            client.QueryAsync<Test>(1000).Returns(new[] { data }.AsQueryable());

            var result = await service.QueryAsync<Test>(1000);

            Assert.AreEqual(result.First(), data);
        }

        [TestMethod]
        public async Task TableClientService_ShouldReturnEmptyQueryWhenNotConnected()
        {
            var service = new TableClientService(null);

            var result = await service.QueryAsync<Test>(1000);

            Assert.AreEqual(result.Count(), 0);
        }

        [TestMethod]
        public async Task TableClientService_ShouldQueryByExpression()
        {
            var client = Substitute.For<IAzureStorageContext>();
            var service = new TableClientService(client);
            var data = new Test();
            var filters = new[] { new QueryFilter() };

            client.QueryAsync<Test>(null, Arg.Any<IEnumerable<QueryFilter>>(), 1000).Returns(new[] { data }.AsQueryable());

            var result = await service.QueryAsync<Test>("Prop1 eq 2", 1000);

            Assert.AreEqual(result.First(), data);
        }

        private class Test { }
    }
}
