using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace brok1.Services
{
    public class HandleUpdateService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<HandleUpdateService> _logger;

        public delegate Task OnMessage(Message message);
        public OnMessage messagedelegate;

        public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger)
        {
            _botClient = botClient;
            _logger = logger;

            messagedelegate = new OnMessage(BotOnMessageReceived);
        }

        public async Task EchoAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage),
                UpdateType.Unknown => Nothing(),
                UpdateType.InlineQuery => Nothing(),
                UpdateType.ChosenInlineResult => Nothing(),
                UpdateType.CallbackQuery => BotOnCallbackQuery(update.CallbackQuery),
                UpdateType.ChannelPost => Nothing(),
                UpdateType.EditedChannelPost => Nothing(),
                UpdateType.ShippingQuery => Nothing(),
                UpdateType.PreCheckoutQuery => Nothing(),
                UpdateType.Poll => Nothing(),
                UpdateType.PollAnswer => Nothing(),
                UpdateType.MyChatMember => Nothing(),
                UpdateType.ChatMember => Nothing(),
                _ => Nothing(),
            };

            try
            {
                Task.Run(async() => await handler);
                //await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
                //await HandleErrorAsync(exception);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            Console.WriteLine($"{message.Chat.Title} -=- {message.From.Username}: {message.Text}");

            message.Text = message.Text.Replace("@" + Variables.bot.Username, "");
            string[] splittedText = message.Text.Split(' ');
            Models.User user = await CheckUser(message.From);
            if (message.Text[0] == '/')
            {
                var action = Variables.commands.FirstOrDefault(m => m.Key.Contains(splittedText[0]));
                if (action.Value != null)
                {
                    await action.Value(_botClient, message, user);
                    //Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");
                }
            }
            else
            {
                await HandleCommands.ProcessNotCommand(_botClient, message, user);
            }

        }
        private async Task BotOnCallbackQuery(CallbackQuery callback)
        {
            Console.WriteLine($"Receive callback data: {callback.Data}");

            Models.User user = await CheckUser(callback.From);
            string[] splittedCallback = callback.Data.Split(' ');
            if (callback.Data.Length >= 2)
            {
                var item = Variables.callbacks.FirstOrDefault(m => m.Key == splittedCallback[1]);
                if (item.Value != default)
                {
                    item.Value(_botClient, callback, user);
                }
            }
        }
        private async Task<Models.User> CheckUser(User user)
        {
            return await Task.Run(() =>
            {
                var user1 = Variables.users.FirstOrDefault(m => m.userid == user.Id);

                if (user1 == default)
                {
                    Models.User userToAdd = new Models.User
                    {
                        userid = user.Id,
                        username = user.Username,
                        balance = 0,
                        moneyadded = 0,
                        moneyused = 0,
                    };
                    Variables.users.Add(userToAdd);
                    user1 = Variables.users.Find(m => m == userToAdd);
                    //Variables.db.UpdateOrInsertWordsTable(userToAdd, true);
                }
                return user1;
            });
        }

        private Task Nothing()
        {
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }

    }
}
