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
            Console.WriteLine("Pay/Index logged in");
            Models.User user = Variables.users.FirstOrDefault(m => m.userid == userId);
            if (user != default)
            {
                if (user.paydata.payStatus == Models.Enums.EPayStatus.WaitingForPay)
                {
                    user.balance += user.paydata.payAmount;
                    Console.WriteLine($"{user.paydata.billResponse.Status.ValueEnum}");
                    user.paydata = new Models.PayData();
                    string sendText = $"Благодарим вас за платеж! На ваш баланс было успешно перечислено {user.paydata.payAmount} рублей.";
                    Variables.botClient.SendTextMessageAsync(userId, sendText);
                    Console.WriteLine($"Pay is done. {user.userid}, amount {user.paydata.payAmount}");
                }
            }
            return Ok();
        }
    }
}
