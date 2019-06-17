using System.Threading.Tasks;

using MentorBot.Functions;
using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.App;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.AzureFunctions
{
    [TestClass]
    [TestCategory("Functions")]
    public sealed class CommandsTests
    {
        #pragma warning disable CS4014

        [TestMethod]
        public async Task GetMessagesStatisticsAsyncShouldQueryDocument()
        {
            var connector = Substitute.For<IOpenAirConnector>();

            ServiceLocator.DefaultInstance.BuildServiceProviderWithDescriptors(
                new ServiceDescriptor(typeof(IOpenAirConnector), connector));

            await Commands.SyncUsersAsync(new TimerInfo(null, null, false));

            connector.Received().SyncUsersAsync();
        }

        #pragma warning restore CS4014
    }
}
