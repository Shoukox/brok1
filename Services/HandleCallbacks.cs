using brok1.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace brok1.Services
{
    public class HandleCallbacks
    {
        //public static async Task moneyAdd(ITelegramBotClient bot, CallbackQuery callback, Models.User user)
        //{
        //    string[] splittedCallback = callback.Data.Split(" ");

        //    string answer = splittedCallback[2];
        //    await bot.DeleteMessageAsync(callback.Message.Chat.Id, callback.Message.MessageId);
        //    if (answer == "yes")
        //    {
        //        user.stage = Models.Enums.EStage.moneyAddAnsweredYes;

        //        string sendText = "Отлично! Дождитесь сообщения от нашего модератора.";
        //        await bot.SendTextMessageAsync(user.userid, sendText);

        //        await Task.Run(() =>
        //        {
        //            string sendText = Langs.GetLang("ru").notifyAdminAboutUserWantsToPay();
        //            var ik = new InlineKeyboardMarkup(
        //                    new InlineKeyboardButton[] {
        //                        new InlineKeyboardButton("Да") {CallbackData = $"{user.userid} userWantsToPay yes"},
        //                        new InlineKeyboardButton("Нет") {CallbackData = $"{user.userid} userWantsToPay no"}
        //                    }
        //                );
        //            Variables.payusers.Add(new Models.PayUsers { user = Variables.users.FirstOrDefault(m => m.userid == user.userid)});
        //            foreach (var item in Variables.WHITELIST)
        //            {
        //                bot.SendTextMessageAsync(item, sendText, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: ik);
        //            }
        //        });
        //    }
        //    else
        //    {
        //        string sendText = "Все хорошо! Вам точно повезет завтра)";
        //        await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
        //    }
        //}
        //public static async Task userWantsToPay(ITelegramBotClient bot, CallbackQuery callback, Models.User user)
        //{
        //    string[] splittedCallback = callback.Data.Split(" ");

        //    string answer = splittedCallback[2];
        //    Models.PayUsers payUser = Variables.payusers.FirstOrDefault(m => m.user.userid == long.Parse(splittedCallback[0]));

        //    await bot.DeleteMessageAsync(callback.Message.Chat.Id, callback.Message.MessageId);
        //    if (answer == "yes")
        //    {
        //        if(payUser.moderator == default)
        //        {
        //            payUser.moderator = Variables.users.FirstOrDefault(m => m.userid == callback.From.Id);
        //            string sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").notifyAdminAboutUserWantsToPayConfirmation(), new[] { $"{payUser.user.userid}", $"тык" });
        //            await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
        //            await bot.SendTextMessageAsync(payUser.user.userid, $"Вам скоро напишет наш <a href=\"tg://user?id={payUser.moderator.userid}\">модератор</a>", Telegram.Bot.Types.Enums.ParseMode.Html);
        //        }
        //        else
        //        {
        //            string sendText = $"За него уже взялся <a href=\"tg://user?id={payUser.moderator.userid}\">он</a>";
        //            await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
        //        }
        //    }
        //}
        public static async Task moneyAdd(ITelegramBotClient bot, CallbackQuery callback, Models.User user)
        {
            string[] splittedCallback = callback.Data.Split(" ");

            string answer = splittedCallback[2];
            await bot.DeleteMessageAsync(callback.Message.Chat.Id, callback.Message.MessageId);
            if (answer == "yes")
            {
                user.stage = Models.Enums.EStage.moneyAddAnsweredYes;
                user.paydata.payStatus = Models.Enums.EPayStatus.WaitingForAmount;
                string sendText = "На какую сумму вы хотите пополнить счёт?\n(Отправьте сумму сообщением)";
                await bot.SendTextMessageAsync(user.userid, sendText);
                //sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").money_billCreated(), new[] { $"{response.PayUrl.AbsoluteUri}" });
            }
        }
        public static async Task moneyPay(ITelegramBotClient bot, CallbackQuery callback, Models.User user)
        {
            string[] splittedCallback = callback.Data.Split(" ");

            await bot.DeleteMessageAsync(callback.Message.Chat.Id, callback.Message.MessageId);
            if (user.paydata.payStatus == Models.Enums.EPayStatus.WaitingForConfirmation)
            {
                user.stage = Models.Enums.EStage.moneyAddAnsweredYes;
                user.paydata.payStatus = Models.Enums.EPayStatus.WaitingForPay;
                string sendText = Langs.ReplaceEmpty(Langs.GetLang("ru").money_billCreated(), new[] { $"{user.paydata.billResponse.Amount.ValueString}", $"{user.paydata.billResponse.PayUrl.AbsoluteUri}" });
                await bot.SendTextMessageAsync(user.userid, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
        public static async Task shop(ITelegramBotClient bot, CallbackQuery callback, Models.User user)
        {
            string[] splittedCallback = callback.Data.Split(" ");
            string[] item = splittedCallback[2].Split("_");
            int count = int.Parse(item[0]);
            int amount = int.Parse(item[1]);

            if(user.balance < amount)
            {
                string sendText = "Недостаточно средств на балансе";
                await bot.SendTextMessageAsync(callback.Message.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }
            user.balance -= amount;
            user.moneyused += amount;
            user.spins += count;
            Variables.db.UpdateOrInsertWordsTable(user, false);
            await bot.SendTextMessageAsync(callback.Message.Chat.Id, $"Вы успешно купили за <b>{amount}</b> рублей следующее количество круток: <b>{count}</b>", Telegram.Bot.Types.Enums.ParseMode.Html);
        }
        public static async Task moonout(ITelegramBotClient bot, CallbackQuery callback, Models.User user)
        {
            string[] splittedCallback = callback.Data.Split(" ");
            string answer = splittedCallback[2];

            bool isAllDigit = true;
            string text = callback.Message.Text.Split("\n")[1].Replace(" ", "").Replace("+", "");
            foreach(char chr in text)
            {
                if (!char.IsDigit(chr))
                    isAllDigit = false;
            }

            if (answer == "yes" && isAllDigit && user.stage == Models.Enums.EStage.waitingQiwiNumberConfirmation)
            {
                user.stage = Models.Enums.EStage.Other;
                user.moons -= 1;
                Other.NotifyAdmins(bot, $"Запрос на вывод.\nUserId: {user.userid}\nСообщения с кошельком:\n\n{callback.Message.Text}");
                string sendText = "Отлично. С вашего баланса списана 1 Луна. Ваши данные были переданы модераторам бота. Ожидайте перевода в течении дня.";
                await bot.SendTextMessageAsync(callback.Message.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            else
            {
                string sendText = "Повторите ввод. Если вы считаете, что это ошибка, перезапустите бота - /restart";
                await bot.SendTextMessageAsync(callback.Message.Chat.Id, sendText, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            Variables.db.UpdateOrInsertWordsTable(user, false);
        }
    }
}
