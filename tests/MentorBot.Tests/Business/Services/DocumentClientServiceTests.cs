using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using MentorBot.Functions.Services;

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Services
{
    [TestClass]
    [TestCategory("Business.Services")]
    public class DocumentClientServiceTests
    {
        private IDocumentClient _documentClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _documentClient = Substitute.For<IDocumentClient>();
        }

        [TestMethod]
        public void DocumentClient_CheckConnectionPolicy()
        {
            Assert.AreEqual(ConnectionMode.Direct, DocumentClientService.Policy.ConnectionMode);
            Assert.AreEqual(Protocol.Tcp, DocumentClientService.Policy.ConnectionProtocol);
            Assert.AreEqual(30, DocumentClientService.Policy.RetryOptions.MaxRetryWaitTimeInSeconds);
        }

        [TestMethod]
        public void DocumentClient_IsConnectedCanBeFalse()
        {
            var client = new DocumentClientService("DB", null);
            Assert.IsFalse(client.IsConnected);
        }

        [TestMethod]
        public void DocumentClient_GetReturnsDocument()
        {
            var client = new DocumentClientService(new Lazy<IDocumentClient>(_documentClient));
            var doc = client.Get<Test>("D", "C");
            Assert.IsInstanceOfType(doc, typeof(DocumentClientService.Document<Test>));
        }

#pragma warning disable CS4014
        [TestMethod]
        public async Task Document_AddAsyncCallsClient()
        {
            var model = new Test();
            var uri = UriFactory.CreateDocumentCollectionUri("DB", "DOC");
            var doc = new DocumentClientService.Document<Test>(_documentClient, "DB", "DOC");

            await doc.AddOrUpdateAsync(model);

            _documentClient.Received().UpsertDocumentAsync(uri, model);
        }

        [TestMethod]
        public void Document_QueryCallsClient()
        {
            var uri = UriFactory.CreateDocumentCollectionUri("DB", "DOC");
            var doc = new DocumentClientService.Document<Test>(_documentClient, "DB", "DOC");
            var models = new[] { new Test() }.AsQueryable();

            _documentClient.CreateDocumentQuery<Test>(uri, "SEL", null).Returns(models);

            var result = doc.Query("SEL");

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task Document_AddManyCallsSP()
        {
            var model = new Test();
            var models = new[] { model };
            var uri = UriFactory.CreateStoredProcedureUri("DB", "DOC", "bulkImport");
            var doc = new DocumentClientService.Document<Test>(_documentClient, "DB", "DOC");

            _documentClient.ExecuteStoredProcedureAsync<int>(uri).ReturnsForAnyArgs(new StoredProcedureResponse<int>());

            await doc.AddManyAsync(models);

            _documentClient.Received().ExecuteStoredProcedureAsync<int>(uri, Arg.Is(CheckModel(model)));
        }

        [TestMethod]
        public async Task Document_UpdateManyCallsSP()
        {
            var model = new Test();
            var models = new[] { model };
            var uri = UriFactory.CreateStoredProcedureUri("DB", "DOC", "bulkUpdate");
            var doc = new DocumentClientService.Document<Test>(_documentClient, "DB", "DOC");

            _documentClient.ExecuteStoredProcedureAsync<int>(uri).ReturnsForAnyArgs(new StoredProcedureResponse<int>());

            await doc.UpdateManyAsync(models);

            _documentClient.Received().ExecuteStoredProcedureAsync<int>(uri, Arg.Is(CheckModel(model)));
        }

#pragma warning restore CS4014

        private class Test { }

        private Expression<Predicate<dynamic[]>> CheckModel(Test model) =>
            it => (it.First() as Test[])[0] == model;
    }
}
