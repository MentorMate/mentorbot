using MentorBot.Api;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace MentorBot.Tests.Api
{
    [TestClass]
    public class StartupTests
    {
        private Startup _startup;

        [TestInitialize]
        public void Init()
        {
            _startup = new Startup(null);
        }

        [TestMethod]
        public void ConfigureServicesShouldAddMvc()
        {
            var services = Substitute.For<IServiceCollection>();

            _startup.ConfigureServices(services);

            services.Received().AddMvc();
            services.ReceivedWithAnyArgs().AddSwaggerGen(null);
        }

        [TestMethod]
        public void ConfigureShouldUseMvc()
        {
            var host = WebHost.CreateDefaultBuilder()
                   .UseStartup<Startup>()
                   .Build();

            // Setup builder
            var builder = Substitute.For<IApplicationBuilder>();
            builder.ApplicationServices.Returns(host.Services);
            builder.ServerFeatures.Returns(host.ServerFeatures);
            
            _startup.Configure(builder);
        }
    }
}