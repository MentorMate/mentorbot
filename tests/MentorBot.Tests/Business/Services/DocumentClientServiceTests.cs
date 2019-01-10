using System;
using System.Threading.Tasks;

using MentorBot.Functions.Services;

using Microsoft.Azure.Documents;
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
        public async Task GetAsyncEnsureDatabaseAndCollectionCreation()
        {
           await _service.GetAsync<Test>("A", "B");

            _documentClient
                .Received()
                .CreateDatabaseIfNotExistsAsync(Arg.Is<Database>(it => it.Id == "A"));

            _documentClient
                .Received()
                .CreateDocumentCollectionIfNotExistsAsync(Arg.Any<Uri>(), Arg.Is<DocumentCollection>(it => it.Id == "B"));
        }

#pragma warning restore CS4014

        private class Test { }
    }
}
