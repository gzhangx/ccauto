using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class ProcessorDonation
    {
        private ProcessingContext context;
        DateTime lastProcessDate = DateTime.Now.AddMinutes(-2);
        public ProcessorDonation(ProcessingContext ctx)
        {
            context = ctx;
        }
        public void ProcessDonate( CommandInfo cmd)
        {
            if (cmd.cmpRes > 2)
            {
                return;
            }

            //ImgChecksAndTags("donatebutton1.png", "INFO_DonateButtonFound", Point(51,19)),

            context.MoveMouseAndClick(cmd.x, cmd.y);            

            bool found = false;
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(2000);
                context.DebugLog("DEBUGPRINTINFO trying to find donation button");
                var results = Utils.GetAppInfo();

                var donationFormat = results.FirstOrDefault(r=>r.command == "INFO_DonateButtonFound");
                if (donationFormat != null)
                {
                    found = true;
                    Utils.MoveMouseAndClick(context.mouse, donationFormat.x, donationFormat.y);
                    break;
                }
            }

            if (found)
            {
                found = false;                
                for (int i = 0; i < 5; i++)
                {                                        
                    context.DebugLog("DEBUGPRINTINFO trying to find archier or wizard for donation");
                    Thread.Sleep(2000);
                    var results = Utils.GetAppInfo();
                    foreach (var donationName in new String[] { "INFO_DonateWizard" }) {
                        var donationFormat = results.FirstOrDefault(r => r.command == donationName);
                        if (donationFormat != null)
                        {
                            found = true;
                            Utils.MoveMouseAndClick(context.mouse, donationFormat.x, donationFormat.y);
                            for (int j = 0; j < 5; j++)
                            {
                                Utils.MouseClick(context.mouse);
                            }

                            found = true;
                        }
                    }
                    if (found) break;
                }
                
            }
            
        }
    }
}
