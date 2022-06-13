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
            language = lang switch
            {
                "ru" => new ru(),
                _ => new ru(),
            };
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
                "Бот - рулетка, в котором вы сможете выбить «Благословение полой Луны».\nТак-же, у вас есть одна бесплатная крутка каждый день!\n(Подробная информация о бесплатной системе прокруток: /info )";
            public string button_balance() =>
                "<b>Ваш баланс</b>\n\n" +
                "————————————\n" +
                "<b>ID</b>: <code>{}</code>\n" +
                "<b>Баланс</b>: {} руб.\n" +
                "<b>ЛУНЫ</b>: {}\n" +
                "<b>Крутки</b>: {}\n" +
                "————————————\n\n" +
                "<b>Всего добавлено:</b> {}р\n" +
                "<b>Всего потрачено:</b> {}р";
            public string button_roulette() =>
                "Здесь ты можешь получить <b>«Благословение полой Луны»</b>\n\n" +
                "С каждым днем твой шанс становится все выше и выше!\nИспытай свою удачу сегодня!";
            public string button_help() =>
                "Если возникнут вопросы, пишите сюда: @okyshonok";
            public string button_info() =>
                "<b>Ежедневная прокрутка:</b>\n\n" +
                "Каждый день вам даётся одна бесплатная прокрутка.\n" +
                "Шанс на выигрыш составляет 2%, с каждой последующей прокруткой этот шанс повышается на 0.1%\n\n" +
                "<b>Доп.прокрутка:</b>\n" +
                "С дополнительных прокруток шанс увеличивается\n" +
                "• С двух + 2%\n" +
                "• С пяти + 6%\n\n" +
                "После выпадения Благословения Полой Луны, шанс сбрасывается обратно до 2%";
            public string button_moneyAdd() =>
                "Для пополнения доступен ТОЛЬКО кошелек QIWI.\n" +
                "Если вас интересует другой способ, обращайтесь к администратору @Okyshonok\n\n" +
                "------------------------\n" +
                "СТОИМОСТЬ ДОП. ПРОКРУТОК:\n" +
                "1 прокрутка - 50 рублей\n" +
                "2 прокруток - 100 рублей\n" +
                "5 прокруток - 250 рублей\n" +
                "------------------------\n\n" +
                "После того, как вы выполните платеж, приобретите доп. прокрутки в магазине ( /shop ).\n\n" +
                "Чтобы пополнить баланс, я выпишу вам счет в Qiwi на ту сумму, которую вы укажете.\n\n" +
                "Вы хотите начать процесс пополнения баланса?";
            public string button_moneyOut() =>
                "Введите свой номер QIWI для вывода «Благословения Полой Луны» / 450р";

            public string roulette_win() =>
                "Поздравляю, ты выиграл Луну(450р)\nЧтобы ее вывести, вернись в меню -> нажми на «баланс» -> «вывести»";
            public string roulette_lose() =>
                "Тебе не повезло, возвращайся завтра за бесплатной круткой ☺️";
            public string roulette_limit() =>
                "Сегодня вы уже пользовались прокруткой! Приходите через {}ч";

            public string shop_item() =>
                "<b>Предмет</b>: {}\n" +
                "<b>Ваш баланс</b>: {} руб.\n" +
                "<b>Стоимость</b>: {} руб.\n\n" +
                "Вы хотите купить этот товар?";

            public string money_billCreated() =>
                "Выставляю счет на {} рублей. Чтобы оплатить, перейдите по ссылке и оплачивайте:\n<a href=\"{}\">Оплатить счет</a>";
            public string money_billInfo() =>
                "Информация о счете:\n" +
                "------------------------\n" +
                "<b>Кошелек</b>: {}\n" +
                "<b>Оплачиваемая сумма:</b> {} рублей\n" +
                "<b>ID оплачиваемого аккаунта:</b> {}\n" +
                "------------------------\n\n" +
                "Если вы готовы к оплате, нажмите на ОПЛАТИТЬ";
            public string money_billCanceled() => "Предыдущий запрос на пополнение был успешно отклонен.";

            public string notifyAdminAboutUserWantsToPay() => "Кто-то хочет оплатить себе на баланс. Вы хотите взяться за него?";
            public string notifyAdminAboutUserWantsToPayConfirmation() => "Отлично. Напишите ему: <a href =\"tg://user?id={}\">{}</a>";

            public string error_restartBot() => "Произошла ошибка. Перезапустите бота, используя /restart";
            public string error_noMoons() => "У вас нет лун";
        }
    }
}
