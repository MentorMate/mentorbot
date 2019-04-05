// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.IO;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Connectors.Luis;
using MentorBot.Functions.Connectors.OpenAir;
using MentorBot.Functions.Models.Options;
using MentorBot.Functions.Processors;
using MentorBot.Functions.Services;
using MentorBot.Localize;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MentorBot.Functions.App
{
#pragma warning disable S1200 // Classes should not be coupled to too many other classes (Single Responsibility Principle)
    /// <summary>Service locator is normally bad practice, but other methods are not realiable in Azure Functions.</summary>
    public class ServiceLocator
    {
        /// <summary>Gets the default service locator instance.</summary>
        public static ServiceLocator DefaultInstance { get; } = new ServiceLocator();

        /// <summary>Gets the service provider.</summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>Configure the service provider if not configured</summary>
        public static void EnsureServiceProvider()
        {
            if (DefaultInstance.ServiceProvider == null)
            {
                DefaultInstance.BuildServiceProviderWithDescriptors();
            }
        }

        /// <summary>Get a service.</summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        public static T Get<T>() =>
            DefaultInstance.ServiceProvider.GetService<T>();

        /// <summary>Build the service provider with additional descriptors.</summary>
        public void BuildServiceProviderWithDescriptors(params ServiceDescriptor[] descriptors)
        {
            var services = ConfigureServices();
            foreach (var descriptor in descriptors)
            {
                services.Insert(services.Count, descriptor);
            }

            ServiceProvider = services.BuildServiceProvider(false);
        }

        private static IServiceCollection ConfigureServices()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true, false)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton(new AzureCloudOptions(config));
            services.AddSingleton(new GoogleCloudOptions(config));
            services.AddSingleton(new OpenAirOptions(config));
            services.AddSingleton<IDocumentClientService>(
                new DocumentClientService(config["AzureCosmosDBAccountEndpoint"], config["AzureCosmosDBKey"]));

            services.AddSingleton<Func<TimeZoneInfo>>(
                () => TimeZoneInfo.FindSystemTimeZoneById(config["DefaultTimeZoneName"]));
            services.AddSingleton<Func<DateTime>>(
                () => DateTime.Now);

            services.AddTransient<IBlobStorageConnector, AzureBlobStorageConnector>();
            services.AddTransient<IAsyncResponder, HangoutsChatConnector>();
            services.AddTransient<IGoogleCalendarConnector, GoogleCalendarConnector>();
            services.AddTransient<IOpenAirConnector, OpenAirConnector>();
            services.AddTransient<ILanguageUnderstandingConnector, AzureLuisConnector>();
            services.AddTransient<IHangoutsChatService, HangoutsChatService>();
            services.AddTransient<ICognitiveService, CognitiveService>();
            services.AddTransient<ICommandProcessor, LocalTimeProcessor>();
            services.AddTransient<ICommandProcessor, RepeatProcessor>();
            services.AddTransient<ICommandProcessor, CalendarProcessor>();
            services.AddTransient<ICommandProcessor, OpenAirProcessor>();
            services.AddTransient<IStringLocalizer, StringLocalizer>();
            services.AddTransient<IStorageService, TableStorageService>();
            services.AddTransient<IOpenAirClient, OpenAirClient>();

            services.AddTransient<LuisClient>();
            services.AddTransient<GoogleServiceAccountCredential>();

            return services;
        }
    }
#pragma warning restore S120
}
