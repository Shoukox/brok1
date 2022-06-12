using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Globalization;

namespace brok1.Services
{
    public class ConfigureWebhook : IHostedService
    {
        private readonly ILogger<ConfigureWebhook> _logger;
        private readonly IServiceProvider _services;
        private readonly BotConfiguration _botConfig;

        public ConfigureWebhook(ILogger<ConfigureWebhook> logger,
                                IServiceProvider serviceProvider,
                                IConfiguration configuration)
        {
            _logger = logger;
            _services = serviceProvider;
            _botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
          
            Variables.users = new List<Models.User>();
            Variables.commands = new Dictionary<string[], Func<ITelegramBotClient, Message, Models.User, Task>>()
            {
                { new string[] {"/start"},   new Func<ITelegramBotClient, Message, Models.User, Task>(HandleCommands.Start)},
                { new string[] {"/restart"},   new Func<ITelegramBotClient, Message, Models.User, Task>(HandleCommands.Restart)},
                { new string[] {"/info"},   new Func<ITelegramBotClient, Message, Models.User, Task>(HandleCommands.Info)},
                { new string[] {"/panel"},   new Func<ITelegramBotClient, Message, Models.User, Task>(HandleCommands.AdminPanel)},
                { new string[] {"/shop"},   new Func<ITelegramBotClient, Message, Models.User, Task>(HandleCommands.Shop)},
                { new string[] {"/test"},   new Func<ITelegramBotClient, Message, Models.User, Task>(HandleCommands.Test)},
            };
            Variables.callbacks = new Dictionary<string, Func<ITelegramBotClient, CallbackQuery, Models.User, Task>>()
            {
                { "moneyAdd", new Func<ITelegramBotClient, CallbackQuery, Models.User, Task>(HandleCallbacks.moneyAdd)},
                { "moneyPay", new Func<ITelegramBotClient, CallbackQuery, Models.User, Task>(HandleCallbacks.moneyPay)},
                { "shop", new Func<ITelegramBotClient, CallbackQuery, Models.User, Task>(HandleCallbacks.shop)},
                { "moonout", new Func<ITelegramBotClient, CallbackQuery, Models.User, Task>(HandleCallbacks.moonout)},
            };
            //Variables.db = new Database("Host=localhost;" +
            //                "Port=1337;" +
            //                "User ID=postgres;" +
            //                "Password=5202340;" +
            //                "Database=brok1;" +
            //                "Pooling=true;" +
            //                //"SSL Mode=Require;" +
            //                "TrustServerCertificate=true;");
            Variables.db = new Database("Host=ec2-34-230-110-100.compute-1.amazonaws.com;" +
                           "Port=5432;" +
                           "User ID=rebmruibddunox;" +
                           "Password=4144be9ea53cb55616a0b25c1c25302fc3d7149da26687260e0eaee14cf630c4;" +
                           "Database=da985ahbb7pct;" +
                           "Pooling=true;" +
                           //"SSL Mode=Require;" +
                           "TrustServerCertificate=true;");
            Variables.qiwi = Qiwi.BillPayments.Client.BillPaymentsClientFactory.Create(Variables.privateQiwiToken);
            Other.LoadData();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
            Variables.bot = await botClient.GetMeAsync();
            Variables.botClient = botClient;
            var webhookAddress = @$"{_botConfig.HostAddress}/bot/{_botConfig.BotToken}";
            Console.WriteLine("Setting webhook: " + webhookAddress);
            await botClient.SetWebhookAsync(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            Console.WriteLine("Removing webhook");
            await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}
