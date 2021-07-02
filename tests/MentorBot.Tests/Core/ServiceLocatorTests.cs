using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Models.Options;
using MentorBot.Functions.Services;
using MentorBot.Localize;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MentorBot.Tests.Core
{
    [TestClass]
    [TestCategory("Core")]
    public class ServiceLocatorTests
    {
        [TestMethod]
        public void ServiceLocatorIsBuild()
        {
            var config = new ConfigurationBuilder().Build();
            var services = new ServiceCollection().ConfigureServices(config).BuildServiceProvider();

            Assert.IsNotNull(services.GetService<GoogleServiceAccountCredential>());
            Assert.IsNotNull(services.GetService<AzureCloudOptions>());
            Assert.IsNotNull(services.GetService<GoogleCloudOptions>());
            Assert.IsInstanceOfType(services.GetService<IDocumentClientService>(), typeof(DocumentClientService));
            Assert.IsInstanceOfType(services.GetService<IBlobStorageConnector>(), typeof(AzureBlobStorageConnector));
            Assert.IsInstanceOfType(services.GetService<IAsyncResponder>(), typeof(HangoutsChatConnector));
            Assert.IsInstanceOfType(services.GetService<IGoogleCalendarConnector>(), typeof(GoogleCalendarConnector));
            Assert.IsInstanceOfType(services.GetService<ICognitiveService>(), typeof(CognitiveService));
            Assert.IsInstanceOfType(services.GetService<IStringLocalizer>(), typeof(StringLocalizer));
        }
    }
}
