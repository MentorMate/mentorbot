using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using MentorBot.Functions.App.Extensions;

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
        public static Task Main() =>
                CreateHostBuilder().Build().RunAsync();
    }
}
