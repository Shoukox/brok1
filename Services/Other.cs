using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using brok1.Models;
using Qiwi.BillPayments.Client;
using Qiwi.BillPayments.Model;
using Qiwi.BillPayments.Model.In;
using Qiwi.BillPayments.Model.Out;
using Telegram.Bot;
using System.Net;
using System.Net.Http;

namespace brok1.Services
{
    public class Other
    {
        public static void LoadData()
        {
            var users = Variables.db.GetData("SELECT * FROM users", 10);

            foreach (var item in users)
            {
                Pseudorandom pseudorandom = new Pseudorandom(2);
                pseudorandom.LoadData((string)item[5]);
                User user = new User
                {
                    userid = (long)item[0],
                    username = (string)item[1],
                    balance = (double)item[2],
                    moneyadded = (double)item[3],
                    moneyused = (double)item[4],
                    pseudorandom = pseudorandom,
                    lastFreeSpin = DateTime.Parse((string)item[6]),
                    spins = (int)item[7],
                    moons = (int)item[8],
                    stoppedBot = (bool)item[9]
                };
                Variables.users.Add(user);
            }

            Console.WriteLine($"users: {users.Count}");
        }
        public static async Task<BillResponse> CreateBill(int amount, long userId)
        {
            Console.WriteLine("starting creating qiwi");
            var createBillInfo = new CreateBillInfo()
            {
                Amount = new MoneyAmount()
                {
                    CurrencyEnum = CurrencyEnum.Rub,
                    ValueDecimal = Convert.ToDecimal(amount),
                },
                BillId = Guid.NewGuid().ToString(),
                SuccessUrl = new Uri($"{Startup.BotConfig.HostAddress}/Pay/Index?userId={userId}"),
                Comment = $"Пополнение баланса в боте",
                ExpirationDateTime = DateTime.Now.AddMinutes(25),
                Customer = new Customer()
                {
                    Account = Guid.NewGuid().ToString(),
                    Email = "shachzod2004@gmail.com",
                    Phone = "998909084147"
                }
            };
            Console.WriteLine($"{createBillInfo.BillId}");
            var billReponse = await Variables.qiwi.CreateBillAsync(createBillInfo);
            return billReponse;
        }
        public static async Task<BillResponse> CheckBill(string billId)
        {
            var billResponse = await Variables.qiwi.GetBillInfoAsync(billId);
            return billResponse;
        }
        public static async Task<BillResponse> CancelBill(string billId)
        {
            var billResponse = await Variables.qiwi.CancelBillAsync(billId);
            return billResponse;
        }
        public static void CheckUsersPay(Models.User user, bool checkedBill = true)
        {
            Console.WriteLine("CheckUsersPay start");
            if (!checkedBill)
            {
                Console.WriteLine("CheckUsersPay 1-if");
                if (user.paydata.billResponse.Status.ValueEnum != BillStatusEnum.Paid)
                {
                    Console.WriteLine("not payed");
                    string sendText = $"Требуемая сумма не была оплачена вовремя. Закрытие запроса пополнения.";
                    Variables.botClient.SendTextMessageAsync(user.userid, sendText);
                    user.paydata.timer.Stop();
                    user.paydata = new PayData();
                }
                else
                {
                    Console.WriteLine("CheckUsersPay 1-if_1");
                    user.balance += user.paydata.payAmount;
                    user.moneyadded += user.paydata.payAmount;
                    string sendText = $"Благодарим вас за платеж! На ваш баланс было успешно перечислено {user.paydata.payAmount} рублей.";
                    user.paydata.timer.Stop();
                    user.paydata = new Models.PayData();
                    Variables.botClient.SendTextMessageAsync(user.userid, sendText);
                    Console.WriteLine($"Pay is done. {user.userid}, amount {user.paydata.payAmount}");
                }
                return;
            }
            if (user.paydata.payStatus == Models.Enums.EPayStatus.WaitingForPay)
            {
                Console.WriteLine("CheckUsersPay 2-if");
                user.balance += user.paydata.payAmount;
                user.moneyadded += user.paydata.payAmount;
                Console.WriteLine($"{user.paydata.billResponse.Status.ValueEnum}");
                string sendText = $"Благодарим вас за платеж! На ваш баланс было успешно перечислено {user.paydata.payAmount} рублей.";
                Console.WriteLine($"Pay is done. {user.userid}, amount {user.paydata.payAmount}");
                user.paydata.timer.Stop();
                user.paydata = new Models.PayData();
                Variables.botClient.SendTextMessageAsync(user.userid, sendText);
            }
            Variables.db.UpdateOrInsertWordsTable(user, false);
            Console.WriteLine("CheckUsersPay end");
        }
        public static void NotifyAdmins(ITelegramBotClient bot, string notify)
        {
            Task.Run(async () =>
            {
                foreach (var item in Variables.WHITELIST)
                {
                    await bot.SendTextMessageAsync(item, notify, Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            });
        }

    }
}
