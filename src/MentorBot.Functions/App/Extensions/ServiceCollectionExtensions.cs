using System;
using System.Text.Json;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Processor;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.SmtpClient;
using MentorBot.Functions.Connectors;
using MentorBot.Functions.Connectors.Base;
using MentorBot.Functions.Connectors.BingMaps;
using MentorBot.Functions.Connectors.Confluence;
using MentorBot.Functions.Connectors.Jenkins;
using MentorBot.Functions.Connectors.Jira;
using MentorBot.Functions.Connectors.Luis;
using MentorBot.Functions.Connectors.OpenAir;
using MentorBot.Functions.Connectors.Wikipedia;
using MentorBot.Functions.Models.Options;
using MentorBot.Functions.Processors;
using MentorBot.Functions.Processors.BuildInfo;
using MentorBot.Functions.Processors.Issues;
using MentorBot.Functions.Processors.Searches;
using MentorBot.Functions.Processors.Timesheets;
using MentorBot.Functions.Processors.UserInfo;
using MentorBot.Functions.Services;
using MentorBot.Functions.Services.AzureStorage;
using MentorBot.Localize;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MentorBot.Functions.App.Extensions
{
    /// <summary>Service locator is normally bad practice, but other methods are not reliable in Azure Functions.</summary>
    public static class ServiceCollectionExtensions
    {
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

        /// <summary>Get a service.</summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        public static T Get<T>(this FunctionContext context) =>
            context.InstanceServices.GetRequiredService<T>();

        /// <summary>Configures the services.</summary>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddMemoryCache();
            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton(new AzureCloudOptions(config));
            services.AddSingleton(new GoogleCloudOptions(config));
            services.AddSingleton(new OpenAirOptions(config));
            services.AddSingleton(new SmtpOptions(config));
            services.AddSingleton(new BingMapsOptions(config));
            services.AddSingleton<IDocumentClientService>(
                new DocumentClientService(config["AzureCosmosDBAccountEndpoint"], config["AzureCosmosDBKey"]));

            services.AddSingleton<Func<TimeZoneInfo>>(
                () => TimeZoneInfo.FindSystemTimeZoneById(config["DefaultTimeZoneName"]));
            services.AddSingleton<Func<DateTime>>(
                () => Contract.LocalDateTime);
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
            services.AddTransient<ICommandProcessor, UserInfoProcessor>();
            services.AddTransient<ICommandProcessor, SearchesProcessor>();
            services.AddTransient<ITimesheetProcessor, OpenAirProcessor>();
            services.AddTransient<IStringLocalizer, StringLocalizer>();
            services.AddTransient<IStorageService, TableStorageService>();
            services.AddTransient<IOpenAirClient, OpenAirClient>();
            services.AddTransient<IAccessTokenService, GoogleAccessTokenService>();
            services.AddTransient<IWikiClient, WikiClient>();
            services.AddTransient<ILuisClient, LuisClient>();
            services.AddTransient<IJenkinsClient, JenkinsClient>();
            services.AddTransient<IJiraClient, JiraClient>();
            services.AddTransient<IBingMapsClient, BingMapsClient>();
            services.AddTransient<ITimesheetService, TimesheetService>();
            services.AddTransient<IConfluenceClient, ConfluenceClient>();

            services.AddHttpClient(JenkinsClient.Name);
            services.AddHttpClient(JiraClient.Name);
            services.AddHttpClient(BingMapsClient.Name);

            services.AddTransient<GoogleServiceAccountCredential>();

            services.Configure<JsonSerializerOptions>(options =>
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

            return services;
        }
    }
}
