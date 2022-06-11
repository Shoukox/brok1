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

namespace brok1.Services
{
    public class HandleCommands
    {
        public static async Task ProcessNotCommand(ITelegramBotClient bot, Message msg, Models.User user)
        {
            string replyCommand = Variables.buttons.FirstOrDefault(m => m == msg.Text);
            if (replyCommand != default)
            {
                switch (replyCommand)
                {
                    case "Баланс":
                        string sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").button_balance(), new[] { $"{user.userid}", $"{user.balance}", $"{user.moneyadded}", $"{user.moneyused}" });
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, replyMarkup: Variables.balanceButtons, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        break;
                    case "Рулетка":
                        sendText = Langs.GetLang("ru").button_roulette();
                        var rk = new ReplyKeyboardMarkup(
                                new KeyboardButton[][]{
                                    new KeyboardButton[]{ new KeyboardButton("Крутить") },
                                    new KeyboardButton[]{ new KeyboardButton("Назад") },
                                }
                            );
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
                                sendText = $"Произошла ошибка. Перезапустите бота, используя /start";
                                await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
                                return;
                            }
                            sendText = $"Предыдущий запрос на пополнение был успешно отклонен.";
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
                        sendText = Langs.GetLang("ru").button_moneyOut();
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId);
                        break;
                    case "Крутить":
                        sendText = "Прокручиваем рулетку";
                        if (user.canSpin)
                        {
                            user.lastSpin = DateTime.Now;
                            Message message = await bot.SendTextMessageAsync(msg.Chat.Id, sendText);
                            for (int i = 1; i <= 3; i++)
                            {
                                sendText += ".";
                                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, sendText);
                                await Task.Delay(2000);
                            }

                            bool win = user.pseudorandom.ProcessChance();
                            if (win)
                            {
                                sendText = Langs.GetLang("ru").roulette_win();
                                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId);
                                break;
                            }
                            else
                            {
                                sendText = Langs.GetLang("ru").roulette_lose();
                                await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId);
                                break;
                            }
                        }
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
                    if (amount <= 10)
                    {
                        sendText = "Слишком маленькая сумма";
                        await bot.SendTextMessageAsync(msg.Chat.Id, sendText);
                        return;
                    }
                    user.paydata.payAmount = amount;
                    user.paydata.payStatus = Models.Enums.EPayStatus.WaitingForConfirmation;
                    Console.WriteLine($"creating response qiwi");
                    var response = await Other.CreateBill(amount, $"{user.userid}{DateTime.Now}");
                    Console.WriteLine($"response not null: {response != null}");
                    user.paydata.billResponse = response;
                    var ik = new InlineKeyboardMarkup(
                            new InlineKeyboardButton("Оплатить") { CallbackData = $"{user.userid} moneyPay" }
                        );
                    sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").money_billInfo(), new[] { $"QIWI", $"{user.paydata.payAmount}", $"{user.userid}" });
                    Console.WriteLine($"{sendText} sendtext passed");
                    await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: ik);
                }
            }
        }

        public static async Task Start(ITelegramBotClient bot, Message msg, Models.User user)
        {
            foreach (var item in Variables.users)
            {
                item.paydata = new Models.PayData();
            }
            string sendText = Langs.GetLang("ru").command_start();
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, replyToMessageId: msg.MessageId, replyMarkup: Variables.startButtons);
        }
        public static async Task Info(ITelegramBotClient bot, Message msg, Models.User user)
        {
            string sendText = Langs.GetLang("ru").button_info();
            await bot.SendTextMessageAsync(msg.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyToMessageId: msg.MessageId, replyMarkup: Variables.startButtons);
        }

        public static async Task Test(ITelegramBotClient bot, Message msg, Models.User user)
        {
            bool hasWon = user.pseudorandom.ProcessChance();
            await bot.SendTextMessageAsync(msg.Chat.Id, $"{hasWon}\nchance: {user.pseudorandom.chance}\nwon: {user.pseudorandom.success}, lost: {user.pseudorandom.loss}");
        }
    }
}
