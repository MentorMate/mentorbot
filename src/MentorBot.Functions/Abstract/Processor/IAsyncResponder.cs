// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using Google.Apis.HangoutsChat.v1.Data;

using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>A responder that sends asyncronus messages.</summary>
    public interface IAsyncResponder
    {
        /// <summary>Sends async message back to the caht room or person.</summary>
        Task SendMessageAsync(string text, GoogleChatAddress address, params Card[] cards);
    }
}
