// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using MentorBot.Core.Abstract.Services;
using MentorBot.Core.Models.HangoutsChat;

namespace MentorBot.Business.Services
{
    /// <summary>The default service handaling 'Hangout Chat' events.</summary>
    /// <seealso cref="IHangoutsChatService"/>
    public class HangoutsChatService : IHangoutsChatService
    {
        /// <inheritdoc/>
        public object Basic(ChatEvent chatEvent)
        {
            return new { text = "Echo " + chatEvent?.Message.Text ?? string.Empty };
        }
    }
}
