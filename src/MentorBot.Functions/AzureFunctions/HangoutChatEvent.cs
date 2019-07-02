// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
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
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

        /// <summary>The main Azure function.</summary>
        [FunctionName("chatEvent")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethods.Post), Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            Debug.Write(req);
            ServiceLocator.EnsureServiceProvider();
            var storageService = ServiceLocator.Get<IStorageService>();

            var content = req.Content ?? throw new ArgumentNullException(nameof(req));
            var hangoutsChatService = ServiceLocator.Get<IHangoutsChatService>();
            var options = ServiceLocator.Get<GoogleCloudOptions>();

            var hangoutChatEventString = await content.ReadAsStringAsync();
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug(hangoutChatEventString);
            }

            var hangoutChatEvent = JsonConvert.DeserializeObject<ChatEvent>(hangoutChatEventString, JsonSettings);

            if (!hangoutChatEvent.Token.Equals(options.HangoutChatRequestToken, StringComparison.InvariantCulture))
            {
                log.LogError("The tokens do not match. Unauthorized access. " + hangoutChatEvent.Token);
                return new UnauthorizedResult();
            }

            var result = await hangoutsChatService
                .BasicAsync(hangoutChatEvent)
                .ConfigureAwait(false);

            if (storageService != null)
            {
                await storageService
                        .SaveMessageAsync(result)
                        .ConfigureAwait(false);
            }

            log.LogInformation(result.Output?.Text);

            return new JsonResult(result.Output, JsonSettings);
        }
    }
}
