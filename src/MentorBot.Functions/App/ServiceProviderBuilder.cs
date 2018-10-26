// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Models.Options;
using MentorBot.Functions.Processors;
using MentorBot.Functions.Services;
using MentorBot.Localize;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace MentorBot.Functions.App
{
    /// <summary>Configure application services.</summary>
    [ExcludeFromCodeCoverage]
    public class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="ServiceProviderBuilder"/> class.</summary>
        public ServiceProviderBuilder(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public IServiceProvider Build()
        {
            var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
            var services = new ServiceCollection();

            services.AddSingleton(loggerFactory);
            services.AddTransient(p => loggerFactory.CreateLogger(string.Empty));
            services.AddTransient<IAsyncResponder, HangoutsChatConnector>();
            services.AddTransient<IHangoutsChatService, HangoutsChatService>();
            services.AddTransient<ICognitiveService, CognitiveService>();
            services.AddTransient<ICommandProcessor, LocalTimeProcessor>();
            services.AddTransient<ICommandProcessor, RepeatProcessor>();
            services.AddTransient<IStringLocalizer, StringLocalizer>();
            services.AddSingleton(
                new GoogleCloudOptions
                {
                    HangoutChatRequestToken = _configuration[nameof(GoogleCloudOptions.HangoutChatRequestToken)],
                    GoogleCloudApplicationName = _configuration[nameof(GoogleCloudOptions.GoogleCloudApplicationName)],
                    GoogleCloudApiKey = _configuration[nameof(GoogleCloudOptions.GoogleCloudApiKey)]
                });

            return services.BuildServiceProvider(true);
        }
    }
}
