using brok1.Models.Enums;
using brok1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brok1.Models
{
    public class User
    {
        public long userid { get; set; }
        public string username { get; set; }
        public double balance { get; set; }
        public double moneyadded { get; set; }
        public double moneyused { get; set; }
        public Pseudorandom pseudorandom { get; set; }
        public DateTime lastSpin { get; set; }
        public DateTime nextSpin
        {
            get
            {
                return lastSpin.AddDays(1);
            }
        }
        public EStage stage { get; set; }
        public PayData paydata { get; set; }
        public bool canSpin
        {
            get
            {
                return (DateTime.Now - lastSpin).Days >= 1;
            }
        }
        public User()
        {
            pseudorandom = new Pseudorandom(2);
            paydata = new PayData();
        }
    }
}
