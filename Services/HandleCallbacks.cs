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
                string sendText = "Отлично. Какую сумму вы хотели бы перевести на свой счет? Отправьте эту сумму сообщением.";
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
    }
}
