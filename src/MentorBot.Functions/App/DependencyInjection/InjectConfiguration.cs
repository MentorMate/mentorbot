// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.IO;

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MentorBot.Functions.App.DependencyInjection
{
    /// <summary>The Inject configurations.</summary>
    public class InjectConfiguration : IExtensionConfigProvider
    {
        /// <inheritdoc/>
        public void Initialize(ExtensionConfigContext context)
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(configuration);

            Startup.RegisterServices(services, configuration);
            Startup.Configure(services, configuration);

            var serviceProvider = services.BuildServiceProvider(true);

            context?
                .AddBindingRule<InjectAttribute>()
                .Bind(new InjectBindingProvider(serviceProvider));
        }
    }
}
