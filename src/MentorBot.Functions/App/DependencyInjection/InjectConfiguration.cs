// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.IO;

using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MentorBot.Functions.App.DependencyInjection
{
    /// <summary>The Inject configurations.</summary>
    public class InjectConfiguration : IExtensionConfigProvider
    {
        /// <inheritdoc/>
        public void Initialize(ExtensionConfigContext context)
        {
            var loggerFactory = context?.Config.GetService<ILoggerFactory>();
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(configuration);
            services.AddSingleton(loggerFactory);
            services.AddTransient(p => loggerFactory.CreateLogger(string.Empty));

            Startup.RegisterServices(services);
            Startup.Configure(services, configuration);

            var serviceProvider = services.BuildServiceProvider(true);

            context?
                .AddBindingRule<InjectAttribute>()
                .Bind(new InjectBindingProvider(serviceProvider));
        }
    }
}
