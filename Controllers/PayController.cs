using brok1.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace brok1.Controllers
{
    public class PayController : Controller
    {
        public IActionResult Index(long userId)
        {

            Console.WriteLine($"Pay/Index logged in, userId={userId}");
            Models.User user = Variables.users.FirstOrDefault(m => m.userid == userId);

            if (user.PayProcessStarted)
                return Ok();

            user.PayProcessStarted = true;
            if (user != default)
            {
                Other.CheckUsersPay(user);
            }
            user.PayProcessStarted = false;
            return Ok();
        }
    }
}
