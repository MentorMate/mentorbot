// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using MentorBot.Core.Models.HangoutsChat;

namespace MentorBot.Core.Abstract.Services
{
    /// <summary>The service that handels hangout chat events.</summary>
    public interface IHangoutsChatService
    {
        /// <summary>An POC for the chat bot.</summary>
        ValueTask<ChatEventResult> BasicAsync(ChatEvent chatEvent);
    }
}
