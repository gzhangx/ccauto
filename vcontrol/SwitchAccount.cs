﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class SwitchAccount : BaseProcessor
    {
        const int ACCOUNTSPACING = 72;
        protected int account = 0;
        const int MAXACCOUNT = 5;
        public SwitchAccount(ProcessingContext ctx) : base(ctx)
        {
            switchSteps = new List<StepInfo>
            {
                new StepInfo { cmd = "-match settingsbutton.png 10", maxRetry = 2, name = "FindSettingsButton", xoff = 10, yoff = 10 },
                new StepInfo { cmd = "-match googlePlaySignIn.png 700", maxRetry = 3, name = "FindPlaySignin", xoff = 101, yoff = 63 },
                new StepInfo { cmd = "-match googlePlayDisconnected.png 700", maxRetry = 3, name = "FindPlaySigninDisconnected", xoff = 101, yoff = 63 },
                new StepInfo { cmd = "-match accountlist.png 6000", maxRetry = 50, name = "SwitchAccount", xoff = 107, yoff = 89, Act = SwitchAccountAction },


                new StepInfo { cmd = "-match loadVillage.png 700", maxRetry = 30, name = "LoadVillage", xoff = 298, yoff = 44 },
                new StepInfo { cmd = "-match confirm.bmp 700", maxRetry = 30, name = "ConfirmLoadVillage", xoff = 310, yoff = 22, Act= ConfirmLoadVillage },          
            };
        }

        public override void Process()
        {            
            Console.WriteLine("Trying to find SINGLEMATCH for settings button");
            switchSteps.First(r => r.name == "SwitchAccount").Act = SwitchAccountAction;
            DoSteps(switchSteps);
            InitGame("Account_" + account);
        }

        private void SwitchAccountAction(CommandInfo found, StepInfo cur)
        {
            if (account >= MAXACCOUNT) account = 0;

            context.MoveMouseAndClick(found.x + cur.xoff, found.y + cur.yoff + (account* ACCOUNTSPACING));
            context.MouseMouseTo(0, 0);

            account++;
            if (account >= MAXACCOUNT) account = 0;
        }

        private void ConfirmLoadVillage(CommandInfo found, StepInfo cur)
        {
            context.MoveMouseAndClick(found.x + cur.xoff, found.y + cur.yoff);
            context.MouseMouseTo(0, 0);
            context.SendString("CONFIRM");

            Thread.Sleep(1000);
            context.mouse.PutMouseEvent(200, 0, 0, 0, 0);
            context.MouseClick();
        }
        List<StepInfo> switchSteps;

    }
}