using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MentorBot.Functions.AzureFunctions
{
    /// <summary>
    /// A function to manage app settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Gets or sets the MentorBot settings from/to Table Storage.
        /// </summary>
        /// <param name="req">The HttpRequest</param>
        /// <param name="log">A logger</param>
        /// <returns>HttpStatus.Ok with the settings object if the request is HttpMethod.Get otherwise status Ok or BadRequest</returns>
        [FunctionName("settings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethod.Get), nameof(HttpMethod.Post), Route = null)] HttpRequest req,
            ILogger log)
        {
            ServiceLocator.EnsureServiceProvider();

            var storageService = ServiceLocator.Get<IStorageService>();

            if (string.Compare(req.Method, nameof(HttpMethod.Post), true) == 0)
            {
                log.LogInformation("Saving settings");
                var body = req.Body ?? throw new ArgumentNullException(nameof(req));
                using (StreamReader reader = new StreamReader(body))
                {
                    var requestBody = await reader.ReadToEndAsync();
                    var settings = JsonConvert.DeserializeObject<MentorBotSettings>(requestBody);
                    await storageService.SaveSettingsAsync(settings);

                    return new OkResult();
                }
            }
            else if (string.Compare(req.Method, nameof(HttpMethod.Get), true) == 0)
            {
                log.LogInformation("Getting settings");
                var result = await storageService.GetSettingsAsync();

                return new OkObjectResult(result);
            }

            return new BadRequestObjectResult("Unsupported method");
        }
    }
}
