using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brok1.Models.Enums
{
    public enum EPayStatus
    {
        Started = 0,
        WaitingForPay = 1,
        Success = 2,
        NoPay = 3,
        WaitingForAmount = 4,
        WaitingForConfirmation = 5
    }
}
