using brok1.Models.Enums;
using Qiwi.BillPayments.Model.Out;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brok1.Models
{
    public class PayData
    {
        public EPayStatus payStatus { get; set; }
        public int payAmount { get; set; }
        public BillResponse billResponse { get; set; }
    }
}
