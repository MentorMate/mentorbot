using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.App.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Hosting;

namespace MentorBot.Functions.App
{
    /// <summary>The application entry point class.</summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        /// <summary>Build host settings.</summary>
        public static IHostBuilder CreateHostBuilder() =>
            new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((ctx, services) =>
                    services.ConfigureServices(ctx.Configuration));

        /// <summary>Main application entry point method.</summary>
        public static Task Main(string[] args) =>
                CreateHostBuilder().FixLinuxStartup(args).Build().RunAsync();

        private static IHostBuilder FixLinuxStartup(this IHostBuilder hostBuilder, string[] args) =>
            hostBuilder.ConfigureAppConfiguration(builder =>
            {
                // Linux: remove the first argument if it's the executable name
                var newArgs = args;
                if (args.Length > 0 && args[0] == typeof(Program).Assembly.Location)
                {
                    newArgs = new string[args.Length - 1];
                    Array.Copy(args, 1, newArgs, 0, args.Length - 1);
                }

                // reorder the configuration source, so that the command line arguments take precedences over the env variables
                var sources = builder.Sources;
                var toRemoves = sources
                    .Where(s =>
                        (s is EnvironmentVariablesConfigurationSource env && env.Prefix == null) ||
                        (s is CommandLineConfigurationSource))
                    .ToList();

                foreach (var toRemove in toRemoves)
                {
                    sources.Remove(toRemove);
                }

                builder.AddEnvironmentVariables();
                builder.AddCommandLine(newArgs);
            });
    }
}
