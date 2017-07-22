using System;
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
        public SwitchAccount(ProcessingContext ctx) : base(ctx) {}

        public override void Process()
        {            
            Console.WriteLine("Trying to find SINGLEMATCH for settings button");
            DoSteps(switchSteps);
            return;
            var found = FindSpot("-match settingsbutton.png 10");
            if (found != null)
            {
                Switch(found);
                return;
            }
        }

        const string matchDisconnect = "-match googlePlayDisconnected.png 2500";
        const string matchAccountList = "-match accountlist.png 2500";
        List<StepInfo> switchSteps = new List<StepInfo>
        {
            new StepInfo { cmd = "-match settingsbutton.png 10", maxRetry = 2, name = "FindSettingsButton", xoff = 10, yoff = 10 },
            new StepInfo { cmd = "-match googlePlaySignIn.png 700", maxRetry = 3, name = "FindPlaySignin", xoff = 101, yoff = 63 },
            new StepInfo { cmd = "-match googlePlayDisconnected.png 700", maxRetry = 3, name = "FindPlaySigninDisconnected", xoff = 101, yoff = 63 },
            new StepInfo { cmd = "-match accountlist.png 700", maxRetry = 30, name = "SwitchAccount", xoff = 107, yoff = 89 },
        };
        protected void Switch(CommandInfo cmd)
        {
            context.MoveMouseAndClick(cmd.x + 10, cmd.y + 10);
            var found = FindSpot("-match googlePlaySignIn.png 500", 1);
            if (found != null)
            {
                context.MoveMouseAndClick(found.x + 101, found.y + 63);
                found = FindSpot(matchDisconnect);                                
            }else
            {
                found = FindSpot(matchDisconnect);
            }
            if (found != null)
            {
                context.MoveMouseAndClick(found.x + 101, found.y + 63);                                
            }

            found = FindSpot(matchAccountList);
            if(found != null)
            {

            }
        }
    }
}
