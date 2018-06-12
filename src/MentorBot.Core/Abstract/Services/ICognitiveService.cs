// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using MentorBot.Core.Models.HangoutsChat;
using MentorBot.Core.Models.TextAnalytics;

namespace MentorBot.Core.Abstract.Services
{
    /// <summary>The congnitive service translate a sentance and extract the most likely command the sentance require.</summary>
    public interface ICognitiveService
    {
        /// <summary>Processes the chat event asynchronous.</summary>
        Task<CognitiveTextAnalysisResult> ProcessAsync(ChatEvent chatEvent);
    }
}
