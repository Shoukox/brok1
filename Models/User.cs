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
        public DateTime lastFreeSpin { get; set; }
        public DateTime nextFreeSpin
        {
            get
            {
                return lastFreeSpin.AddDays(1);
            }
        }
        public EStage stage { get; set; }
        public PayData paydata { get; set; }
        public int spins { get; set; }
        public int moons { get; set; }
        public bool PayProcessStarted { get; set; } //paycontroller //other.userspay
        public bool isSpinning { get; set; }
        public bool canFreeSpin
        {
            get
            {
                return DateTime.Now >= nextFreeSpin;
            }
        }
        public User()
        {
            pseudorandom = new Pseudorandom(2);
            paydata = new PayData();
        }
    }
}
