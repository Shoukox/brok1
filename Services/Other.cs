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
            var users = Variables.db.GetData("SELECT * FROM users", 5);

            foreach (var item in users)
            {
                User user = new User { userid = (long)item[0], username = (string)item[1], balance = (double)item[2], moneyadded = (double)item[3], moneyused = (double)item[4] };
                Variables.users.Add(user);
            }

            Console.WriteLine($"users: {users.Count}");
        }
        public static async Task<BillResponse> CreateBill(int amount, string billId, long userId)
        {
            Console.WriteLine("starting creating qiwi");
            var createBillInfo = new CreateBillInfo()
            {
                Amount = new MoneyAmount()
                {
                    CurrencyEnum = CurrencyEnum.Rub,
                    CurrencyString = "RUB",
                    ValueDecimal = Convert.ToDecimal(amount),
                    ValueString = $"{amount}"
                },
                BillId = billId,
                SuccessUrl = new Uri($"{Startup.BotConfig.HostAddress}/Pay/Index?userId={userId}"),
                Comment = $"bill {billId} {amount}",
                ExpirationDateTime = DateTime.Now.AddMinutes(20)
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
        public static async Task CheckUsersPay(Models.User user, bool checkedBill = true)
        {
            if (!checkedBill)
            {
                if (user.paydata.billResponse.Status.ValueEnum != BillStatusEnum.Paid)
                    Console.WriteLine("not payed");
                    return;
            }
            if (user.paydata.payStatus == Models.Enums.EPayStatus.WaitingForPay)
            {
                user.balance += user.paydata.payAmount;
                Console.WriteLine($"{user.paydata.billResponse.Status.ValueEnum}");
                user.paydata = new Models.PayData();
                string sendText = $"Благодарим вас за платеж! На ваш баланс было успешно перечислено {user.paydata.payAmount} рублей.";
                await Variables.botClient.SendTextMessageAsync(user.userid, sendText);
                Console.WriteLine($"Pay is done. {user.userid}, amount {user.paydata.payAmount}");
            }
        }

    }
}
