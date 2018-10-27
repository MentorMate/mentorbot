// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.DataResultModels;
using MentorBot.Functions.Models.Domains;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MentorBot.Functions
{
    /// <summary>Application query functions.</summary>
    public static class Queries
    {
        /// <summary>The main Azure function.</summary>
        [FunctionName("get-messages-stats")]
        public static async Task<IEnumerable<MessagesStatistic>> GetMessagesStatisticsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Get), Route = null)] HttpRequest req)
        {
            Debug.Write(req);

            ServiceLocator.EnsureServiceProvider();

            var client = ServiceLocator.Get<IDocumentClientService>() ?? throw new NullReferenceException();

            var document = await client.GetAsync<Message>("mentorbot", "messages").ConfigureAwait(false);

            var messages = document
                .Query("SELECT TOP 1000 m.ProbabilityPercentage FROM messages m")
                .GroupBy(it => it.ProbabilityPercentage / 10)
                .Select(group => new MessagesStatistic
                {
                    ProbabilityPercentage = (byte)(group.Key * 10),
                    Count = group.Count()
                });

            return messages;
        }
    }
}
