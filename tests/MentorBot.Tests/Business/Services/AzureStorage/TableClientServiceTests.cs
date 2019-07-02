using System;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Services.AzureStorage;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services.AzureStorage
{
    [TestClass]
    [TestCategory("Business.Services")]
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
            await client.Received().CreateTableAsync<Test>(true);
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
            await client.Received().CreateTableAsync<Test>(true);
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
        public void TableClientService_ShouldAddSchema()
        {
            var client = Substitute.For<IAzureStorageContext>();
            var service = new TableClientService(client);
            var data = typeof(Test);

            service.AddAttributeMapper(new[] { data });

            client.Received().AddAttributeMapper(data);
        }

        private class Test { }
    }
}
