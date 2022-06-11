using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brok1.Localization
{
    public interface ILocalization
    {
        public string command_start();
        public string button_balance();
        public string button_roulette();
        public string button_help();
        public string button_info();
        public string button_moneyAdd();
        public string button_moneyOut();

        public string money_billCreated();
        public string money_billInfo();

        public string roulette_win();
        public string roulette_lose();

        public string notifyAdminAboutUserWantsToPay();
        public string notifyAdminAboutUserWantsToPayConfirmation();
    }
}
