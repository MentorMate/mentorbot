// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Diagnostics.CodeAnalysis;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(MentorBot.Functions.App.Startup))]
namespace MentorBot.Functions.App
{
    /// <summary>The application startup and setup class.</summary>
    [ExcludeFromCodeCoverage]
    internal class Startup : IWebJobsStartup
    {
        /// <inheritdoc/>
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDependencyInjection<ServiceProviderBuilder>();
        }
    }
}
