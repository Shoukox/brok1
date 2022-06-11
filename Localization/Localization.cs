using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brok1.Localization
{
    public class Langs
    {
        public static ILocalization GetLang(string lang)
        {
            ILocalization language = new ru();
            switch (lang)
            {
                case "ru":
                    language = new ru();
                    break;
                default:
                    language = new ru();
                    break;
            }
            return language;
        }
        public static string ReplaceEmpty(string text, string[] replace)
        {
            foreach (var item in replace)
            {
                int ind = text.IndexOf("{}");
                text = text.Remove(ind, 2).Insert(ind, item);
            }
            return text;
        }
        public class ru : ILocalization
        {
            public string command_start() =>
                "Я бот. Бот - рулетка, в котором вы сможете выбить «Благословение полой Луны». Каждый день у вас есть одна бесплатная крутка!.\n" +
                "Подробная информация о системе прокруток: /info";
            public string button_balance() =>
                "<b>Ваш баланс</b>\n" +
                "\n" +
                "\n" +
                "<b>ID</b>: {}\n" +
                "<b>Баланс</b>: {}\n" +
                "\n" +
                "=========\n" +
                "\n" +
                "<b>Всего добавлено:</b> {}\n" +
                "<b>Всего потрачено:</b> {}";
            public string button_roulette() =>
                "РУЛЕТКА\n\n" +
                "Здесь ты можешь получить <b>«Благословение полой Луны»</b>\n\nС каждым днем твой шанс становится все выше и выше!\nИспытай свою удачу сегодня!";
            public string button_help() =>
                "Если возникнут вопросы, пишите сюда - @okyshonok";
            public string button_info() =>
                "Каждый день вы будете получать 1 бесплатную прокрутку. Изначальный шанс на выигрыш составляет 2%. Этот шанс с каждой последущей прокруткой будет повышаться на 0.1%";
            public string button_moneyAdd() =>
                "Для пополнения доступен ТОЛЬКО кошелек QIWI.\n" +
                "Если вас интересует другой способ, обращайтесь к администратору @x_Broken_x\n" +
                "------------------------\n" +
                "СТОИМОСТЬ ДОП. ПРОКРУТОК:\n" +
                "1 прокрутка - 100 рублей\n" +
                "5 прокруток - 450 рублей\n" +
                "10 прокруток - 800 рублей\n" +
                "После того, как вы оплатите баланс, приобретайте прокрутки в магазине (/shop).\n" +
                "Чтобы пополнить баланс, я выпишу вам счет в Qiwi на ту сумму, которую вы укажете. Вы хотите начать процесс пополнения баланса?";
            public string button_moneyOut() =>
                "vivodim dengi";

            public string roulette_win() =>
                "Поздравляю, ты везунчик, тебе выпала Луна(450р) 🤑";
            public string roulette_lose() =>
                "Тебе не повезло, возвращайся завтра за бесплатной круткой ☺️";

            public string money_billCreated() =>
                "Выставляю счет на {} рублей. Чтобы оплатить, перейдите по ссылке и оплачивайте:\n<a href=\"{}\">Оплатить счет</a>";
            public string money_billInfo() =>
                "Информация о счете:\n" +
                "------------------------\n" +
                "<b>Кошелек</b>: {}\n" +
                "<b>Оплачиваемая сумма:</b> {}\n" +
                "<b>ID оплачиваемого аккаунта:</b> {}\n" +
                "------------------------\n\n" +
                "Если вы готовы к оплате, нажмите на ОПЛАТИТЬ";

            public string notifyAdminAboutUserWantsToPay() => "Кто-то хочет оплатить себе на баланс. Вы хотите взяться за него?";
            public string notifyAdminAboutUserWantsToPayConfirmation() => "Отлично. Напишите ему: <a href =\"tg://user?id={}\">{}</a>";
            
        }
    }
}
