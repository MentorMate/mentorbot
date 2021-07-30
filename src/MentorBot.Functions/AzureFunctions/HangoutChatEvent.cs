using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.Options;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace MentorBot.Functions
{
    /// <summary>The application entry point for Hangout chat event.</summary>
    public static class HangoutChatEvent
    {
        /// <summary>The main Azure function.</summary>
        [Function("chatEvent")]
        public static async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = null)] HttpRequestData req,
            FunctionContext context)
        {
            Debug.Write(req);

            var storageService = context.Get<IStorageService>();
            var log = context.GetLogger(nameof(HangoutChatEvent));
            var hangoutsChatService = context.Get<IHangoutsChatService>();
            var options = context.Get<GoogleCloudOptions>();
            var hangoutChatEvent = await req.ReadAsAsync<ChatEvent>();

            log.LogDebug(
                $"Message from '{hangoutChatEvent.Message.Sender.DisplayName}' is '{hangoutChatEvent.Message.Text}'.");

            if (!hangoutChatEvent.Token.Equals(options.HangoutChatRequestToken, StringComparison.InvariantCulture))
            {
                log.LogError("The tokens do not match. Unauthorized access. " + hangoutChatEvent.Token);
                return req.CreateResponse(HttpStatusCode.Unauthorized);
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

            return await req.CreateContentResponseAsync(result.Output);
        }
    }
}
