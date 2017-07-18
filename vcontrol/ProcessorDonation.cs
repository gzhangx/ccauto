﻿using System;
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
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(2000);
                Console.WriteLine("DEBUGPRINTINFO trying to find donation button");
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
                var results = Utils.GetAppInfo();
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine("DEBUGPRINTINFO trying to find archier or wizard for donation");
                    

                    foreach (var donationName in new String[] { "INFO_DonateWizard", "INFO_DonateArchier" }) {
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
                
                {
                    var close = results.FirstOrDefault(r => r.command == "STDCLICK_Close");
                    if (close != null)
                    {
                        Utils.MoveMouseAndClick(context.mouse, close.x, close.y);
                    }
                }
            }
            
        }
    }
}
