// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using MentorBot.Business.Processors;
using MentorBot.Business.Services;

using MentorBot.Core.Abstract.Processor;
using MentorBot.Core.Abstract.Services;
using MentorBot.Core.Localize;
using MentorBot.Core.Models.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MentorBot.Functions.App
{
    /// <summary>The application startup and setup class.</summary>
    public static class Startup
    {
        /// <summary>Registers the services.</summary>
        internal static void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IHangoutsChatService, HangoutsChatService>();
            services.AddTransient<ICognitiveService, CognitiveService>();
            services.AddTransient<ICommandProcessor, LocalTimeProcessor>();
            services.AddTransient<IStringLocalizer, StringLocalizer>();
        }

        /// <summary>Configures the specified services.</summary>
        internal static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(
                new GoogleCloudOptions
                {
                    HangoutChatRequestToken = configuration[nameof(GoogleCloudOptions.HangoutChatRequestToken)]
                });
        }
    }
}
