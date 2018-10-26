// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using MentorBot.Functions.Models.HangoutsChat;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>A responder that sends asyncronus messages.</summary>
    public interface IAsyncResponder
    {
        /// <summary>Sends async message back to the caht room or person.</summary>
        Task SendMessageAsync(string text, ChatEventSpace space, ChatEventMessageThread thread, ChatEventMessageSender sender);
    }
}
