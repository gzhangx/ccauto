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

            //ImgChecksAndTags("donatebutton1.png", "INFO_DonateButtonFound", Point(51,19)),
            var processed = new List<CommandInfo>();
            for (int i = 0; i < 5; i++)
            {
                context.MoveMouseAndClick(cmd.x, cmd.y);            
                bool found = false;
                //Thread.Sleep(2000);
                Utils.doScreenShoot(ProcessorMapByText.tempImgName);
                context.DebugLog("DEBUGPRINTINFO trying to find donation button");
                //-matchRect 227,102,140,600_200
                const int donateRectx = 227;
                const int donateRecty = 102;
                var results = Utils.GetAppInfo($"-input {ProcessorMapByText.tempImgName} -name donate -matchRect {donateRectx},{donateRecty},140,600_200 -top 5  -match data\\check\\donatebutton.png 250");                
                foreach (var donate in results)
                {
                    if (donate.decision == "true")
                    {
                        if (processed.Any(p =>
                        {
                            return Math.Abs(p.y - donate.y) < 20;
                        })) continue;
                        processed.Add(donate);
                        found = true;
                        context.MoveMouseAndClick(donateRectx + donate.x + 10, donateRecty + donate.y + 10);

                        for (int dwretry = 0; dwretry < 2; dwretry++)
                        {
                            Utils.doScreenShoot(ProcessorMapByText.tempImgName);                            
                            var dw = Utils.GetAppInfo($"-input {ProcessorMapByText.tempImgName} -name dw  -match data\\check\\donate_wizard.png 200");
                            var dwbtn = dw.FirstOrDefault(dwf => dwf.decision == "true");
                            if (dwbtn != null)
                            {
                                context.MoveMouseAndClick(50 + dwbtn.x, 50 + dwbtn.y);
                                for (int cli = 0; cli< 5; cli++) context.MouseClick();
                                break;
                            }
                        }
                    }
                    results = Utils.GetAppInfo();
                    context.DoStdClicks(results);
                }                
                if (!found) break;
            }            
        }
    }
}
