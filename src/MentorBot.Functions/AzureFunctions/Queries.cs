// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App;
using MentorBot.Functions.Models.DataResultModels;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace MentorBot.Functions
{
    /// <summary>Application query functions.</summary>
    public static class Queries
    {
        /// <summary>The main Azure function.</summary>
        [FunctionName("get-messages-stats")]
        public static IEnumerable<MessagesStatistic> GetMessagesStatistics(
            [HttpTrigger(AuthorizationLevel.Anonymous, nameof(HttpMethods.Get), Route = null)] HttpRequest req)
        {
            Debug.Write(req);

            ServiceLocator.EnsureServiceProvider();

            var storage = ServiceLocator.Get<IStorageService>() ?? throw new NullReferenceException();

            var messages = storage
                .GetMessages()
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
