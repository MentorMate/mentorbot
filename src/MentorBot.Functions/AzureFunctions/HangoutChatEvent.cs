// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MentorBot.Functions
{
    /// <summary>The application entry point for Hangout chat event.</summary>
    public static class HangoutChatEvent
    {
        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

        /// <summary>The main Azure function.</summary>
        [FunctionName("chatEvent")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Post), Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            Debug.Write(req);
            ServiceLocator.EnsureServiceProvider();

            var content = req.Content ?? throw new ArgumentNullException(nameof(req));
            var client = ServiceLocator.Get<IDocumentClientService>();
            var hangoutsChatService = ServiceLocator.Get<IHangoutsChatService>();
            var options = ServiceLocator.Get<GoogleCloudOptions>();

            var hangoutChatEvent = await content
                .ReadAsAsync<ChatEvent>()
                .ConfigureAwait(true);

            if (!hangoutChatEvent.Token.Equals(options.HangoutChatRequestToken, StringComparison.InvariantCulture))
            {
                log.LogError("The tokens do not match. Unauthorized access. " + hangoutChatEvent.Token);
                return new UnauthorizedResult();
            }

            if (log.IsEnabled(LogLevel.Debug))
            {
                var contentAsString = await content.ReadAsStringAsync();
                log.LogDebug(contentAsString);
            }

            var result = await hangoutsChatService
                .BasicAsync(hangoutChatEvent)
                .ConfigureAwait(false);

            if (client.IsConnected)
            {
                log.LogDebug("Add message to document database mentorbot.");

                var document = await client
                    .GetAsync<Message>("mentorbot", "messages")
                    .ConfigureAwait(false);

                await document
                    .AddAsync(result)
                    .ConfigureAwait(false);
            }

            log.LogInformation(result.Output?.Text);

            return new JsonResult(result.Output, JsonSettings);
        }
    }
}
