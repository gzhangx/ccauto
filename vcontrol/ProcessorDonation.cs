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

            var donatedPos = new List<CommandInfo>();
            //ImgChecksAndTags("donatebutton1.png", "INFO_DonateButtonFound", Point(51,19)),
            var processed = new List<CommandInfo>();
            for (int i = 0; i < 5; i++)
            {
                context.MoveMouseAndClick(cmd.x, cmd.y);            
                bool found = false;
                context.Sleep(2000);
                Utils.doScreenShoot(ProcessorMapByText.tempImgName);
                context.DebugLog("DEBUGPRINTINFO trying to find donation button");
                //-matchRect 227,102,140,600_200
                const int donateRectx = 227;
                const int donateRecty = 102;
                var results = Utils.GetAppInfo($"-input {ProcessorMapByText.tempImgName} -name donate -matchRect {donateRectx},{donateRecty},140,600_200 -top 5  -match data\\check\\donatebutton.png 350", context);
                foreach (var donate in results)
                {
                    if (donatedPos.Any(dp => dp.y == donate.y)) continue;
                    donatedPos.Add(donate);
                    if (donate.decision == "true")
                    {
                        if (processed.Any(p =>
                        {
                            return Math.Abs(p.y - donate.y) < 20;
                        })) continue;
                        processed.Add(donate);
                        found = true;
                        context.Sleep(100);
                        context.MoveMouseAndClick(donateRectx + donate.x + 55, donateRecty + donate.y + 23);
                        context.Sleep(1000);
                        for (int dwretry = 0; dwretry < 2; dwretry++)
                        {
                            Utils.doScreenShoot(ProcessorMapByText.tempImgName);
                            var dw = Utils.GetAppInfo($"-input {ProcessorMapByText.tempImgName} -name dw  -match data\\check\\donate_wizard.png 300", context);
                            var dwbtn = dw.FirstOrDefault(dwf => dwf.decision == "true");
                            if (dwbtn != null)
                            {
                                context.MoveMouseAndClick(50 + dwbtn.x, 50 + dwbtn.y);
                                for (int cli = 0; cli < 5; cli++) context.MouseClick();
                                break;
                            }
                        }
                        context.DoStdClicks();
                    }
                }
                if (!found) break;
            }            
        }
    }
}
