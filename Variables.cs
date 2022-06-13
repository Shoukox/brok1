using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Qiwi.BillPayments.Client;


namespace brok1
{
    public class Variables
    {
        public static Dictionary<string[], Func<ITelegramBotClient, Message, Models.User, Task>> commands;
        public static Dictionary<string, Func<ITelegramBotClient, CallbackQuery, Models.User, Task>> callbacks;
        public static List<Models.User> users;

        public static List<long> WHITELIST = new List<long> { 2002832238, 728384906, 358798501, 776098531, 1448214492};

        public static Database db;
        public static User bot;
        public static ITelegramBotClient botClient;
        public static BillPaymentsClient qiwi;

        public static ReplyKeyboardMarkup startButtons = new ReplyKeyboardMarkup(
                new KeyboardButton[][]{
                        new KeyboardButton[] { new KeyboardButton("Баланс"),  new KeyboardButton("Рулетка")},
                        new KeyboardButton[] { new KeyboardButton("Тех. поддержка"), new KeyboardButton("Информация")},
                        new KeyboardButton[] { new KeyboardButton("Магазин")}
                    }
            ){ ResizeKeyboard = true };
        public static ReplyKeyboardMarkup balanceButtons = new ReplyKeyboardMarkup(
               new KeyboardButton[][]{
                        new KeyboardButton[] { new KeyboardButton("Пополнить"), new KeyboardButton("Вывести") },
                        new KeyboardButton[] { new KeyboardButton("Назад") }
                   }
           ) { ResizeKeyboard = true };
        public static ReplyKeyboardMarkup ShopButtons = new ReplyKeyboardMarkup
                (
                    new KeyboardButton[][]
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("1 крутка за 50р"),
                            new KeyboardButton("2 крутки за 100р"),
                            new KeyboardButton("5 круток за 250р"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Назад"),
                        }
                    }
                ){ ResizeKeyboard = true };
        public static string[] adminFuncs = new[] { "editbalance", "editrandom", "editmoon" };
        public static string[] buttons = new[] { "Баланс", "Рулетка", "Тех. поддержка", "Информация", "Пополнить", "Вывести", "Назад", "Крутить", "Магазин", "1 крутка за 50р", "2 крутки за 100р", "5 круток за 250р" };

        public static string qiwiToken = "3599523b7912c1a6cca174ead91fcaf1";
        public static string publicQiwiToken = "48e7qUxn9T7RyYE1MVZswX1FRSbE6iyCj2gCRwwF3Dnh5XrasNTx3BGPiMsyXQFNKQhvukniQG8RTVhYm3iP5HoWvxahg8wy7DRg23M2hFL7fyV6DH4V6hawCPTS6T7BY7GHdHyBSG7jVrtdwrigUxDhUNQ4z4MyFiBvg1VUy5ujNuM1tn6uHTpjvK3LV";
        public static string privateQiwiToken = "eyJ2ZXJzaW9uIjoiUDJQIiwiZGF0YSI6eyJwYXlpbl9tZXJjaGFudF9zaXRlX3VpZCI6IjYxbnlmaS0wMCIsInVzZXJfaWQiOiI3OTE0Mjk2MDE5MCIsInNlY3JldCI6ImM4N2RlY2Q2NmZmZTAxODIwNjIwMTA2YzFiMTg4MzFkMjI2MTZhNDUyNGQzMjA5NjgwYmNkNDZiNzIzMjg3YzMifX0=";
    }
}
