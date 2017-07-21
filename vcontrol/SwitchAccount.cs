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
        public SwitchAccount(ProcessingContext ctx) : base(ctx) {}

        public override void Process()
        {
            Console.WriteLine("Trying to find SINGLEMATCH for settings button");
            var found = FindSpot("-match settingsbutton.png 10");
            if (found != null)
            {
                Switch(found);
                return;
            }
        }

        const string matchDisconnect = "-match googlePlayDisconnected.png 500";
        const string matchAccountList = "-match accountlist.png 500";
        protected void Switch(CommandInfo cmd)
        {
            context.MoveMouseAndClick(cmd.x + 10, cmd.y + 10);
            var found = FindSpot("-match googlePlaySignIn.png 10", 1);
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
