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
            if ((DateTime.Now - lastProcessDate).TotalMinutes > 1)
            {
                lastProcessDate = DateTime.Now;
            }
            else return;

            Utils.MouseMouseTo(context.mouse, cmd.x, cmd.y);
            Utils.MouseClick(context.mouse);

            bool found = false;
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(2000);
                Console.WriteLine("DEBUGPRINTINFO trying to find donation button");
                var results = Utils.GetAppInfo();

                var donationFormat = results.FirstOrDefault(r=>r.command == "INFO_DonateButtonFound");
                if (donationFormat != null)
                {
                    found = true;
                    Utils.MouseMouseAndClick(context.mouse, donationFormat.x, donationFormat.y);
                    break;
                }
            }

            if (found)
            {
                found = false;
                var results = Utils.GetAppInfo();
                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("DEBUGPRINTINFO trying to find archier or wizard");
                    

                    foreach (var donationName in new String[] { "INFO_DonateWizard", "INFO_DonateArchier" }) {
                        var donationFormat = results.FirstOrDefault(r => r.command == donationName);
                        if (donationFormat != null)
                        {
                            found = true;
                            Utils.MouseMouseAndClick(context.mouse, donationFormat.x, donationFormat.y);
                            for (int j = 0; j < 5; j++)
                            {
                                Utils.MouseClick(context.mouse);
                            }

                            found = true;
                        }
                    }
                }
                if (found)
                {
                    var close = results.FirstOrDefault(r => r.command == "INFO_Close");
                    Utils.MouseMouseAndClick(context.mouse, close.x, close.y);
                }
            }
            
        }
    }
}
