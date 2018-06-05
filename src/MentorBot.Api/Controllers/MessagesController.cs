// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Threading.Tasks;

using MentorBot.Core.Abstract.Services;
using MentorBot.Core.Models.HangoutsChat;

using Microsoft.AspNetCore.Mvc;

namespace MentorBot.Api.Controllers
{
    /// <summary>An endpoint that process messages and commands.</summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IHangoutsChatService _hangoutsChatService;

        /// <summary>Initializes a new instance of the <see cref="MessagesController"/> class.</summary>
        public MessagesController(IHangoutsChatService hangoutsChatService)
        {
            _hangoutsChatService = hangoutsChatService;
        }

        /// <summary>The service that handels chat events.</summary>
        [HttpPost("hangouts")]
        public ValueTask<ChatEventResult> HangoutsChatAsync([FromBody] ChatEvent chatEvent) =>
            _hangoutsChatService.BasicAsync(chatEvent);
    }
}
