using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Models.Options;
using MentorBot.Functions.Services;
using MentorBot.Localize;

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
            ServiceLocator.EnsureServiceProvider();

            Assert.IsNotNull(ServiceLocator.Get<GoogleServiceAccountCredential>());
            Assert.IsNotNull(ServiceLocator.Get<AzureCloudOptions>());
            Assert.IsNotNull(ServiceLocator.Get<GoogleCloudOptions>());

            Assert.IsInstanceOfType(ServiceLocator.Get<IDocumentClientService>(), typeof(DocumentClientService));
            Assert.IsInstanceOfType(ServiceLocator.Get<IBlobStorageConnector>(), typeof(AzureBlobStorageConnector));
            Assert.IsInstanceOfType(ServiceLocator.Get<IAsyncResponder>(), typeof(HangoutsChatConnector));
            Assert.IsInstanceOfType(ServiceLocator.Get<IGoogleCalendarConnector>(), typeof(GoogleCalendarConnector));
            Assert.IsInstanceOfType(ServiceLocator.Get<ICognitiveService>(), typeof(CognitiveService));
            Assert.IsInstanceOfType(ServiceLocator.Get<IStringLocalizer>(), typeof(StringLocalizer));
        }

        [TestMethod]
        public void ServiceLocatorEnsureWithDescriptorsShouldAddDescriptors()
        {
            var serviceLocator = new ServiceLocator();

            serviceLocator.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(Test), new Test()));

            Assert.IsNotNull(serviceLocator.ServiceProvider.GetService<Test>());
        }

        private class Test { }
    }
}
