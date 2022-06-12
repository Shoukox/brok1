using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brok1.Models.Enums
{
    public enum EStage
    {
        Other = 0,
        moneyAddProcessing = 1,
        moneyAddAnsweredYes = 2,
        waitingForQIWINumber = 3,
        waitingQiwiNumberConfirmation = 4

    }
}
