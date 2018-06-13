// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using MentorBot.Core.Abstract.Services;
using MentorBot.Core.Localize;
using MentorBot.Core.Models.HangoutsChat;
using MentorBot.Core.Models.Options;
using MentorBot.Functions.App.DependencyInjection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

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
                [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Post), Route = null)]HttpRequest req,
                [Inject] IHangoutsChatService hangoutsChatService,
                [Inject] GoogleCloudOptions options,
                [Inject] IStringLocalizer localizer,
                TraceWriter log)
        {
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

            log.Info(localizer["C# HTTP trigger function processed a request."]);

            var hangoutChatEvent = GetRequestBody(req, log);

            if (hangoutChatEvent.Token.Equals(options.HangoutChatRequestToken, StringComparison.InvariantCulture))
            {
                log.Error(localizer["The tokens do not match. Unauthorized access."]);
                return new UnauthorizedResult();
            }

            var result = await hangoutsChatService.BasicAsync(hangoutChatEvent).ConfigureAwait(false);

            log.Info(result?.Text);

            return new JsonResult(result, JsonSettings);
        }

        private static ChatEvent GetRequestBody(HttpRequest req, TraceWriter log)
        {
            if (req == null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            using (var requestBodyStream = new StreamReader(req.Body))
            {
                var requestBodyString = requestBodyStream.ReadToEnd();

                log?.Info($"Validate request {requestBodyString}.");

                return JsonConvert.DeserializeObject<ChatEvent>(requestBodyString);
            }
        }
    }
}
