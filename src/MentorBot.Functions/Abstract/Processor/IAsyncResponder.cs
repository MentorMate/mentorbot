using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>A responder that sends asynchronous messages.</summary>
    public interface IAsyncResponder
    {
        /// <summary>Sends async message back to the chat room or person.</summary>
        Task SendMessageAsync(string text, GoogleChatAddress address, params Card[] cards);
    }
}
