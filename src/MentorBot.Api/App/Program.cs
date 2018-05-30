// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MentorBot.Api
{
    /// <summary>The application entry point class.</summary>
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        /// <summary>Main application entry point method.</summary>
        public static void Main(string[] args) =>
            CreateWebHostBuilder(args).Build().Run();

        /// <summary>Creates the web host builder.</summary>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
