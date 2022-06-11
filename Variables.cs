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

        public static List<long> WHITELIST = new List<long> { /*2002832238,*/ 728384906 };

        public static Database db;
        public static User bot;
        public static ITelegramBotClient botClient;
        public static BillPaymentsClient qiwi;

        public static ReplyKeyboardMarkup startButtons = new ReplyKeyboardMarkup(
                new KeyboardButton[][]{
                    new KeyboardButton[] { new KeyboardButton("Баланс")},
                    new KeyboardButton[] { new KeyboardButton("Рулетка") },
                    new KeyboardButton[] { new KeyboardButton("Тех. поддержка") },
                    new KeyboardButton[] { new KeyboardButton("Информация") }
                    }
            );
        public static ReplyKeyboardMarkup balanceButtons = new ReplyKeyboardMarkup(
               new KeyboardButton[][]{
                    new KeyboardButton[] { new KeyboardButton("Пополнить")},
                    new KeyboardButton[] { new KeyboardButton("Вывести") },
                    new KeyboardButton[] { new KeyboardButton("Назад") }
                   }
           );
        public static string[] buttons = new[] { "Баланс", "Рулетка", "Тех. поддержка", "Информация", "Пополнить", "Вывести", "Назад", "Крутить" };

        public static string qiwiToken = "3599523b7912c1a6cca174ead91fcaf1";
        public static string publicQiwiToken = "48e7qUxn9T7RyYE1MVZswX1FRSbE6iyCj2gCRwwF3Dnh5XrasNTx3BGPiMsyXQFNKQhvukniQG8RTVhYm3iPpZQCqD4ek7JzjymAr44qh5QcfRuEdhBr5e1zHzx7RAp7fQUYbEyY3BB8cQ6tXWaq5eUXprfrXd4pvUskVH6LCpbfhvP3JUWgEvX7Sj9bH";
        public static string privateQiwiToken = "eyJ2ZXJzaW9uIjoiUDJQIiwiZGF0YSI6eyJwYXlpbl9tZXJjaGFudF9zaXRlX3VpZCI6ImNtMmE2YS0wMCIsInVzZXJfaWQiOiI5OTg5MzM4NTkyNTgiLCJzZWNyZXQiOiJkMTE4NjdjNTM0MzMzNzNlZDJmYWMyMWRkMmVjNDNjOWM5M2FhZGQzN2E2MTJhMjVlZGIwYjFlZjZhMzcwNmRmIn19";
    }
}
