// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Diagnostics;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.App;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace MentorBot.Functions
{
    /// <summary>Application query functions.</summary>
    public static class Commands
    {
        /// <summary>A sync users command.</summary>
        [FunctionName("sync-users")]
        public static async Task<IActionResult> SyncUsersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Get), Route = null)] HttpRequest req)
        {
            Debug.Write(req);

            ServiceLocator.EnsureServiceProvider();

            var openAirConnector = ServiceLocator.Get<IOpenAirConnector>();

            await openAirConnector.SyncUsersAsync();

            return new JsonResult(new { });
        }
    }
}
