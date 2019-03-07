using System;
using System.Linq;
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
        private DocumentClientService _service;
        private IDocumentClient _documentClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _documentClient = Substitute.For<IDocumentClient>();
            _service = new DocumentClientService(new Lazy<IDocumentClient>(_documentClient));
        }

#pragma warning disable CS4014
        [TestMethod]
        public async Task Document_AddAsyncCallsClient()
        {
            var model = new Test();
            var uri = UriFactory.CreateDocumentCollectionUri("DB", "DOC");
            var doc = new DocumentClientService.Document<Test>(_documentClient, "DB", "DOC");

            await doc.AddAsync(model);

            _documentClient.Received().CreateDocumentAsync(uri, model);
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

#pragma warning restore CS4014

        private class Test { }
    }
}
