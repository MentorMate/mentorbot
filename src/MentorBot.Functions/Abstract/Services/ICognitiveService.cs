using System.Threading.Tasks;

using MentorBot.Functions.Models.HangoutsChat;
using MentorBot.Functions.Models.TextAnalytics;

namespace MentorBot.Functions.Abstract.Services
{
    /// <summary>The cognitive service translate a sentence and extract the most likely command the sentence require.</summary>
    public interface ICognitiveService
    {
        /// <summary>Processes the chat event asynchronous.</summary>
        Task<CognitiveTextAnalysisResult> ProcessAsync(ChatEvent chatEvent);

        /// <summary>Gets the cognitive text analysis result asynchronous.</summary>
        Task<CognitiveTextAnalysisResult> GetCognitiveTextAnalysisResultAsync(TextDeconstructionInformation definition, string email);
    }
}
