// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.SmtpClient;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Connectors.Jenkins;
using MentorBot.Functions.Connectors.Jira;
using MentorBot.Functions.Connectors.Luis;
using MentorBot.Functions.Connectors.OpenAir;
using MentorBot.Functions.Connectors.Wikipedia;
using MentorBot.Functions.Models.Options;
using MentorBot.Functions.Processors;
using MentorBot.Functions.Processors.BuildInfo;
using MentorBot.Functions.Processors.Issues;
using MentorBot.Functions.Processors.Timesheets;
using MentorBot.Functions.Services;
using MentorBot.Functions.Services.AzureStorage;
using MentorBot.Localize;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MentorBot.Functions.App
{
#pragma warning disable S1200 // Classes should not be coupled to too many other classes (Single Responsibility Principle)
    /// <summary>Service locator is normally bad practice, but other methods are not realiable in Azure Functions.</summary>
    public sealed class ServiceLocator
    {
        /// <summary>Gets the default service locator instance.</summary>
        public static ServiceLocator DefaultInstance { get; } = new ServiceLocator();

        /// <summary>Gets the service provider.</summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>Tries to create a type or return default value.</summary>
        /// <typeparam name="T">The type to be created.</typeparam>
        public static T Try<T>(Func<T> creator, T defaultValue = default)
        {
            try
            {
                return creator();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>Configure the service provider if not configured.</summary>
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

        /// <summary>Get a service.</summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        public static IEnumerable<T> GetServices<T>() =>
            DefaultInstance.ServiceProvider.GetServices<T>();

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

        /// <summary>Configures the services.</summary>
        private static IServiceCollection ConfigureServices()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", true, false)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            services.AddMemoryCache();
            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton(new AzureCloudOptions(config));
            services.AddSingleton(new GoogleCloudOptions(config));
            services.AddSingleton(new OpenAirOptions(config));
            services.AddSingleton(new SmtpOptions(config));
            services.AddSingleton<IDocumentClientService>(
                new DocumentClientService(config["AzureCosmosDBAccountEndpoint"], config["AzureCosmosDBKey"]));

            services.AddSingleton<Func<TimeZoneInfo>>(
                () => TimeZoneInfo.FindSystemTimeZoneById(config["DefaultTimeZoneName"]));
            services.AddSingleton<Func<DateTime>>(
                () => DateTime.Now);
            services.AddTransient<IAzureStorageContext, AzureStorageContext>(x =>
                Try(() => new AzureStorageContext(x.GetService<AzureCloudOptions>()?.AzureStorageAccountConnectionString)));

            services.AddTransient<ISmtpClient, SmtpClientBase>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<ITableClientService, TableClientService>();
            services.AddTransient<IBlobStorageConnector, AzureBlobStorageConnector>();
            services.AddTransient<IAsyncResponder, HangoutsChatConnector>();
            services.AddTransient<IHangoutsChatConnector, HangoutsChatConnector>();
            services.AddTransient<IGoogleCalendarConnector, GoogleCalendarConnector>();
            services.AddTransient<IOpenAirConnector, OpenAirConnector>();
            services.AddTransient<ILanguageUnderstandingConnector, AzureLuisConnector>();
            services.AddTransient<IHangoutsChatService, HangoutsChatService>();
            services.AddTransient<ICognitiveService, CognitiveService>();
            services.AddTransient<ICommandProcessor, BuildInfoProcessor>();
            services.AddTransient<ICommandProcessor, LocalTimeProcessor>();
            services.AddTransient<ICommandProcessor, RepeatProcessor>();
            services.AddTransient<ICommandProcessor, CalendarProcessor>();
            services.AddTransient<ICommandProcessor, OpenAirProcessor>();
            services.AddTransient<ICommandProcessor, WikipediaProcessor>();
            services.AddTransient<ICommandProcessor, HelpProcessor>();
            services.AddTransient<ICommandProcessor, HelloProcessor>();
            services.AddTransient<ICommandProcessor, IssuesProcessor>();
            services.AddTransient<ITimesheetProcessor, OpenAirProcessor>();
            services.AddTransient<IStringLocalizer, StringLocalizer>();
            services.AddTransient<IStorageService, TableStorageService>();
            services.AddTransient<IOpenAirClient, OpenAirClient>();
            services.AddTransient<IAccessTokenService, GoogleAccessTokenService>();
            services.AddTransient<IWikiClient, WikiClient>();
            services.AddTransient<ILuisClient, LuisClient>();
            services.AddTransient<IJenkinsClient, JenkinsClient>();
            services.AddTransient<IJiraClient, JiraClient>();
            services.AddTransient<ITimesheetService, TimesheetService>();

            services.AddHttpClient(JenkinsClient.Name);
            services.AddHttpClient(JiraClient.Name);

            services.AddTransient<GoogleServiceAccountCredential>();

            return services;
        }
    }
#pragma warning restore S120
}
