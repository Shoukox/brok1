using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using brok1.Localization;
using Qiwi.BillPayments.Model;
using System.Timers;
using Qiwi.BillPayments.Model.Out;

namespace brok1.Services
{
    public class HandleCommands
    {
        public static async Task ProcessNotCommand(ITelegramBotClient bot, Message msg, Models.User user)
        {
            /* 1. Повторная прокрутка - "Вы уже исчерпали лимит в 1 прокрутку в день"   done
             * 2. Уведомление: "Остался 1 час до вашей бесплатной прокрутки"            canceled
             * 3. Уведомление: "Вы уже можете крутить"                                  canceled 
             * 4. БД: users, admins, bills
             * 5. Админ панель                                                          done
             * 6. Проверка оплаты
             * 7. Оптимизация бота
             * 8. /shop                                                                 done
             * 9. pseudorandom string saving                                            
             */
            string replyCommand = Variables.buttons.FirstOrDefault(m => m == msg.Text);
            if (replyCommand != default)
            {
                switch (replyCommand)
                {
                    case "Баланс":
                        string sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").button_balance(), new[] { $"{user.userid}", $"{user.balance}", $"{user.moons}", $"{user.spins}", $"{user.moneyadded}", $"{user.moneyused}" });
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, replyMarkup: Variables.balanceButtons, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        break;
                    case "Рулетка":
                        sendText = Langs.GetLang("ru").button_roulette();
                        var rk = new ReplyKeyboardMarkup(
                                new KeyboardButton[][]{
                                    new KeyboardButton[]{ new KeyboardButton("Крутить") },
                                    new KeyboardButton[]{ new KeyboardButton("Назад") },
                                }
                            )
                        { ResizeKeyboard = true };
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: rk);
                        break;
                    case "Тех. поддержка":
                        sendText = Langs.GetLang("ru").button_help();
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        break;
                    case "Информация":
                        sendText = Langs.GetLang("ru").button_info();
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        break;
                    case "Назад":
                        sendText = Langs.GetLang("ru").command_start();
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, replyMarkup: Variables.startButtons);
                        break;
                    case "Пополнить":
                        if (user.paydata.payStatus == Models.Enums.EPayStatus.WaitingForPay)
                        {
                            if (user.paydata.billResponse == null)
                            {
                                sendText = Langs.GetLang("ru").error_restartBot();
                                await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
                                return;
                            }

                            await Other.CancelBill(user.paydata.billResponse.BillId);
                            user.paydata = new Models.PayData();
                            sendText = Langs.GetLang("ru").money_billCanceled();
                            await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
                        }
                        user.stage = Models.Enums.EStage.moneyAddProcessing;
                        user.paydata.payStatus = Models.Enums.EPayStatus.Started;
                        sendText = Langs.GetLang("ru").button_moneyAdd();
                        var ik = new InlineKeyboardMarkup(
                                    new InlineKeyboardButton[]
                                    {
                                        new InlineKeyboardButton("Да") { CallbackData=$"{user.userid} moneyAdd yes"},
                                        new InlineKeyboardButton("Нет"){ CallbackData=$"{user.userid} moneyAdd no"},

                                    });
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, replyMarkup: ik);
                        break;
                    case "Вывести":
                        if (user.moons == 0)
                        {
                            sendText = Langs.GetLang("ru").error_noMoons();
                            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId);
                            return;
                        }
                        sendText = Langs.GetLang("ru").button_moneyOut();
                        user.stage = Models.Enums.EStage.waitingForQIWINumber;
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId);
                        break;
                    case "Крутить":
                        sendText = "Прокручиваем рулетку\n";
                        if (user.canFreeSpin || user.spins >= 1 /*|| Variables.WHITELIST.Contains(msg.From.Id)*/)
                        {
                            if (user.isSpinning)
                            {
                                sendText = "Подождите окончания прошлой рулетки";
                                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId);
                                return;
                            }
                            user.isSpinning = true;
                            if (user.canFreeSpin)
                            {
                                user.lastFreeSpin = DateTime.Now;
                            }
                            else if (user.spins >= 1)
                            {
                                user.spins -= 1;
                            }
                            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, sendText);
                            string[] emojis = new[] { "⬜️", "🟨", "🟧", "🟥", "🟩" };
                            for (int i = 0; i <= emojis.Length - 1; i++)
                            {
                                sendText += emojis[i];
                                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, sendText);
                                await Task.Delay(2000);
                            }

                            bool win = user.pseudorandom.ProcessChance();
                            Variables.db.UpdateOrInsertWordsTable(user, false);
                            if (win)
                            {
                                user.moons += 1;
                                sendText = Langs.GetLang("ru").roulette_win();

                                Other.NotifyAdmins(bot, sendText + $"\n\nUserId: {user.userid}\nUserName: {user.username}\n<a href=\"tg://user?id={user.userid}\">Ссылка</a>");

                                user.isSpinning = false;
                                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId);
                                break;
                            }
                            else
                            {
                                sendText = Langs.GetLang("ru").roulette_lose();
                                user.isSpinning = false;
                                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId);
                                break;
                            }
                        }
                        else
                        {
                            sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").roulette_limit(), new[] { $"{(user.nextFreeSpin - DateTime.Now).Hours}" });
                            user.isSpinning = false;
                            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId);
                        }
                        break;
                    case "Магазин":
                        sendText = "Магазин";
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: Variables.ShopButtons, replyToMessageId: msg.MessageId);
                        break;
                    case "1 крутка за 50р":
                        string itemName = "1 крутка";
                        int amount = 50;
                        sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").shop_item(), new[] { $"{itemName}", $"{user.balance}", $"{amount}" });
                        ik = new InlineKeyboardMarkup(new InlineKeyboardButton("Купить") { CallbackData = $"{user.userid} shop 1_50" });
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId, replyMarkup: ik);
                        break;
                    case "2 крутки за 100р":
                        itemName = "2 крутки";
                        amount = 100;
                        sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").shop_item(), new[] { $"{itemName}", $"{user.balance}", $"{amount}" });
                        ik = new InlineKeyboardMarkup(new InlineKeyboardButton("Купить") { CallbackData = $"{user.userid} shop 2_100" });
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId, replyMarkup: ik);
                        break;
                    case "5 круток за 250р":
                        itemName = "5 круток";
                        amount = 250;
                        sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").shop_item(), new[] { $"{itemName}", $"{user.balance}", $"{amount}" });
                        ik = new InlineKeyboardMarkup(new InlineKeyboardButton("Купить") { CallbackData = $"{user.userid} shop 5_250" });
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId, replyMarkup: ik);
                        break;
                }
            }
            else if (user.paydata.payStatus == Models.Enums.EPayStatus.WaitingForAmount)
            {
                int amount = 0;
                if (!int.TryParse(msg.Text, out amount))
                {
                    string sendText = "Вы ввели не число. Введите число оплаты без надписи \"р.\" \"рублей\" и т.д.\n" +
                        "Например: 100";
                    await bot.SendTextMessageAsync(msg.Chat.Id, sendText);
                    return;
                }
                else
                {
                    string sendText;
                    if (amount < 10)
                    {
                        sendText = "Слишком маленькая сумма";
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText);
                        return;
                    }
                    user.paydata.payAmount = amount;
                    user.paydata.payStatus = Models.Enums.EPayStatus.WaitingForConfirmation;
                    Console.WriteLine($"creating response qiwi");

                    BillResponse response;
                    while (true)
                    {
                        try
                        {
                            response = await Other.CreateBill(amount, user.userid);
                            break;
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }


                    Console.WriteLine($"response not null: {response != null}");
                    user.paydata.billResponse = response;
                    user.paydata.timer = new Timer(24000 * 60)
                    {
                        AutoReset = false,
                        Enabled = true
                    };
                    user.paydata.timer.Elapsed += (o, s) =>
                    {
                        try
                        {
                            Console.WriteLine($"Timer elapsed");
                            user.paydata.billResponse = Other.CheckBill(response.BillId).Result;
                            Other.CheckUsersPay(user, false);
                            Console.WriteLine($"Timer elapsed - end");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
                    user.paydata.timer.Start();

                    var ik = new InlineKeyboardMarkup(
                            new InlineKeyboardButton("Оплатить") { CallbackData = $"{user.userid} moneyPay" }
                        );
                    sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").money_billInfo(), new[] { $"QIWI", $"{user.paydata.payAmount}", $"{user.userid}" });
                    await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: ik);

                    Variables.db.UpdateOrInsertWordsTable(user, false);
                }
            }
            else if (user.stage == Models.Enums.EStage.waitingForQIWINumber)
            {
                var ik = new InlineKeyboardMarkup(
                        new InlineKeyboardButton[]{
                        new InlineKeyboardButton("Да"){CallbackData = $"{user.userid} moonout yes"},
                        new InlineKeyboardButton("Нет"){CallbackData = $"{user.userid} moonout no"}
                        });
                string sendText = $"Номер вашей карты Qiwi:\n{msg.Text}\n\nВсе верно?";
                user.stage = Models.Enums.EStage.waitingQiwiNumberConfirmation;
                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId, replyMarkup: ik);
            }
            else
            {
                string sendText = "Неизвестная команда. Используйте /restart, чтобы перезапустить бота.";
                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId);
            }
        }

        public static async Task Start(ITelegramBotClient bot, Message msg, Models.User user)
        {
            string sendText = Langs.GetLang("ru").command_start();
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, replyMarkup: Variables.startButtons);
        }
        public static async Task Restart(ITelegramBotClient bot, Message msg, Models.User user)
        {
            user.paydata = new Models.PayData();
            user.stage = Models.Enums.EStage.Other;
            string sendText = Langs.GetLang("ru").command_start();
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, replyMarkup: Variables.startButtons);
        }
        public static async Task Info(ITelegramBotClient bot, Message msg, Models.User user)
        {
            string sendText = Langs.GetLang("ru").button_info();
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId, replyMarkup: Variables.startButtons);
        }
        public static async Task AdminPanel(ITelegramBotClient bot, Message msg, Models.User user)
        {
            if (!Variables.WHITELIST.Contains(msg.From.Id))
                return;

            string[] splittedText = msg.Text.Split(" ");
            string sendText = "";
            bool isUserName = false;
            if (splittedText.Length != 4 || !Variables.adminFuncs.Contains(splittedText[1]) || (!int.TryParse(splittedText[2], out _) && (splittedText[2][0] != '+' && splittedText[2][0] != '-')) || (!long.TryParse(splittedText[3], out _) && splittedText[3][0] != '@'))
            {
                sendText =
                "Неверное использование команды.\n\n" +
                $"{string.Join("\n", Variables.adminFuncs.Select(m => $"/panel {m} num username\\userId"))}\n\n" +
                $"Вместо <b>num</b> укажите число, на которое хотите заменить. Если вы хотите добавить или отнять от его текущего числа, то вместо <b>num</b> укажите +5 или -5 соответственно.\n" +
                $"Вместо <b>username\\userId</b> укажите либо username (@Shoukkoo), либо userId (728384906), чтобы бот знал, кому нужно что менять.\n\n" +
                $"editmoon - изменение количества лун\n" +
                $"editbalance - изменение баланса\n" +
                $"editrandom - изменение шанса";
                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId);
                return;
            }
            if (splittedText[3][0] == '@')
                isUserName = true;
            Models.User user1 = isUserName ?
                Variables.users.FirstOrDefault(m => m.username.ToLower() == splittedText[3].ToLower()) :
                Variables.users.FirstOrDefault(m => m.userid == long.Parse(splittedText[3]));

            int num = 0;
            switch (splittedText[1])
            {
                case "editbalance":
                    if (splittedText[2][0] == '+')
                    {
                        num = int.Parse(splittedText[2].Substring(1, splittedText[2].Length - 1));
                        user1.balance += num;
                    }
                    else if (splittedText[2][0] == '-')
                    {
                        num = int.Parse(splittedText[2].Substring(1, splittedText[2].Length - 1));
                        user1.balance -= num;
                    }
                    else
                    {
                        num = int.Parse(splittedText[2]);
                        user1.balance = num;
                    }
                    Variables.db.UpdateOrInsertWordsTable(user1, false);
                    break;
                case "editrandom":
                    if (splittedText[2][0] == '+')
                    {
                        num = int.Parse(splittedText[2].Substring(1, splittedText[2].Length - 1));
                        user1.pseudorandom.EditChance(user1.pseudorandom.chance + num);
                    }
                    else if (splittedText[2][0] == '-')
                    {
                        num = int.Parse(splittedText[2].Substring(1, splittedText[2].Length - 1));
                        user1.pseudorandom.EditChance(user1.pseudorandom.chance - num);

                    }
                    else
                    {
                        num = int.Parse(splittedText[2]);
                        user1.pseudorandom.EditChance(num);
                    }
                    Variables.db.UpdateOrInsertWordsTable(user1, false);
                    break;
                case "editmoon":
                    if (splittedText[2][0] == '+')
                    {
                        num = int.Parse(splittedText[2].Substring(1, splittedText[2].Length - 1));
                        user1.moons += num;
                    }
                    else if (splittedText[2][0] == '-')
                    {
                        num = int.Parse(splittedText[2].Substring(1, splittedText[2].Length - 1));
                        user1.moons -= num;

                    }
                    else
                    {
                        num = int.Parse(splittedText[2]);
                        user1.moons = num;
                    }
                    Variables.db.UpdateOrInsertWordsTable(user1, false);
                    break;
            }
            sendText =
                $"Вы использовали {splittedText[1]} на <a href=\"tg://user?id={user1.userid}\">нем</a>\n" +
                $"Его баланс и шанс:\n" +
                $"balance: {user1.balance}\n" +
                $"moons: {user1.moons}\n" +
                $"native_chance: {user1.pseudorandom.chance}\n" +
                $"fact_chance: {user1.pseudorandom.secretChance}";
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId, replyMarkup: Variables.startButtons);
        }
        public static async Task Stats(ITelegramBotClient bot, Message msg, Models.User user)
        {
            if (!Variables.WHITELIST.Contains(msg.From.Id))
                return;

            string sendText = $"users: {Variables.users.Count}";
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId);
        }
        public static async Task Shop(ITelegramBotClient bot, Message msg, Models.User user)
        {
            string sendText = "Магазин";
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: Variables.ShopButtons, replyToMessageId: msg.MessageId);

        }
        public static async Task Test(ITelegramBotClient bot, Message msg, Models.User user)
        {
            bool hasWon = user.pseudorandom.ProcessChance();
            //var response = await Other.CheckBill("2002832238132994379884597814");
            await bot.SendTextMessageAsync(msg.Chat.Id, $"{hasWon}\nnc: {user.pseudorandom.native_chance}, c: {user.pseudorandom.chance}, fc: {user.pseudorandom.secretChance}\nwon: {user.pseudorandom.success}, lost: {user.pseudorandom.loss}");

        }
    }
}
