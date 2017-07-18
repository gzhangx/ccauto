using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class ProcessorTrain
    {
        private ProcessingContext context;
        DateTime lastProcessDate = DateTime.Now.AddMinutes(-2);
        public ProcessorTrain(ProcessingContext ctx)
        {
            context = ctx;
        }
        public void ProcessCommand( CommandInfo cmd)
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
            {
                found = false;
                var results = Utils.GetAppInfo();
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("DEBUGPRINTINFO trying to find archier or wizard to build");
                                        
                    foreach (var donationName in new String[] { "INFO_BuildWizard", "INFO_BuildArcher" }) {
                        var donationFormat = results.FirstOrDefault(r => r.command == donationName);
                        if (donationFormat != null)
                        {
                            Console.WriteLine("Found " + donationName);
                            found = true;
                            Utils.MoveMouseAndClick(context.mouse, donationFormat.x, donationFormat.y);
                            if (donationName == "INFO_BuildWizard")
                                for (int j = 0; j < 5; j++) Utils.MouseClick(context.mouse);
                            else
                                Utils.MouseClick(context.mouse);
                            found = true;
                        }
                    }
                }
                
                {
                    var close = results.FirstOrDefault(r => r.command == "STDCLICK_Close");
                    if (close != null)
                    {
                        Utils.MoveMouseAndClick(context.mouse, close.x, close.y);
                    }
                }
                if (found) Console.WriteLine("units build");
            }
            
        }
    }
}
