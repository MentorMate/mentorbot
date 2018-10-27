// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Options;
using MentorBot.Localize;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

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
            [CosmosDB("mentorbot", "messages", CreateIfNotExists = true, ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Message> messages,
            [Inject] IHangoutsChatService hangoutsChatService,
            [Inject] GoogleCloudOptions options,
            [Inject] IStringLocalizer localizer,
            ILogger log)
        {
            if (req == null)
            {
                return new ObjectResult("Request not initialized")
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            if (hangoutsChatService == null ||
                localizer == null ||
                log == null ||
                options == null)
            {
                return new ObjectResult("Services not initialized")
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            log.LogInformation(localizer["C# HTTP trigger function processed a request."]);

            var hangoutChatEvent = await req
                .Content
                .ReadAsAsync<ChatEvent>()
                .ConfigureAwait(true);

            if (!hangoutChatEvent.Token.Equals(options.HangoutChatRequestToken, StringComparison.InvariantCulture))
            {
                log.LogError(localizer["The tokens do not match. Unauthorized access."]);
                return new UnauthorizedResult();
            }

            var result = await hangoutsChatService
                .BasicAsync(hangoutChatEvent)
                .ConfigureAwait(false);

            if (messages != null)
            {
                await messages.AddAsync(result).ConfigureAwait(false);
            }

            log.LogInformation(result.Output?.Text);

            return new JsonResult(result.Output, JsonSettings);
        }

        /// <summary>The main Azure function.</summary>
        [FunctionName("get-messages-stats")]
        public static IActionResult GetMessagesStatistics(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Get), Route = null)] HttpRequest req,
            [CosmosDB("mentorbot", "messages", ConnectionStringSetting = "CosmosDBConnection", SqlQuery = "SELECT TOP 1000 m.ProbabilityPercentage FROM messages m")] IEnumerable<Message> messages,
            [Inject] IStringLocalizer localizer,
            ILogger log)
        {
            if (localizer == null || req == null)
            {
                return new ObjectResult("Request not initialized")
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            log.LogInformation(localizer["C# HTTP trigger function get-messages-stats."]);

            var result = messages
                .GroupBy(it => it.ProbabilityPercentage / 10)
                .Select(group => new MessagesStatistic
                {
                    ProbabilityPercentage = (byte)(group.Key * 10),
                    Count = group.Count()
                });

            return new JsonResult(result, JsonSettings);
        }
    }
}
