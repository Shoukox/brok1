using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using brok1.Services;
using Telegram.Bot.Types;

namespace brok1.Controllers
{
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromServices] HandleUpdateService handleUpdateService,
                                              [FromBody] Update update)
        {
            await Task.Run(() => handleUpdateService.EchoAsync(update));
            return Ok();
        }
    }
}
