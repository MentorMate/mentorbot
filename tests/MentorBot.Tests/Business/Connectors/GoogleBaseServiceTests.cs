using Google.Apis.Requests;
using Google.Apis.Services;

using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Models.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Business.Connectors
{
    [TestClass]
    [TestCategory("Business.Connectors")]
    public sealed class GoogleBaseServiceTests
    {
        [TestMethod]
        public void CreateInitializer_InitsByApiKay()
        {
            var configuration = Substitute.For<IConfiguration>();

            configuration["GoogleCloudApiKey"].Returns("ABC");
            configuration["GoogleCloudApplicationName"].Returns("EFG");

            var options = new GoogleCloudOptions(configuration);

            var result = GoogleBaseService<BaseClientService>.InitByKey(options);

            Assert.AreEqual("ABC", result.ApiKey);
            Assert.AreEqual("EFG", result.ApplicationName);
        }

        [TestMethod]
        public void GoogleBaseServiceSetup_ShouldSet()
        {
            var req = Substitute.For<IClientServiceRequest>();
            var res = req.Setup(it => it.ExecuteAsStream());

            req.Received().ExecuteAsStream();

            Assert.AreEqual(req, res);
        }
    }
}
