using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class ActionJunkRemove
    {
        public static string tempImgName = StandardClicks.GetTempDirFile("tempJunkRemove.png");
        private AutoResourceLoader junkFinder;
        private AutoResourceLoader buttons;
        private ProcessingContext context;
        public ActionJunkRemove(ProcessingContext ctx)
        {
            context = ctx;
            junkFinder = new AutoResourceLoader(ctx, tempImgName, @"data\check\useless");
            buttons = new AutoResourceLoader(ctx, tempImgName, new string[] {
                @"data\check\useless\btns\removebutton.png",
                @"data\check\useless\btns\cancelremove.png",
            });
        }

        public void Process()
        {
            const int  SLEEPS = 5;
            var junks = junkFinder.Processing();
            foreach (var junk in junks)
            {
                if (junk.x > 200 && junk.decision == "true")
                {
                    context.DebugLog($"Found {junk.extraInfo} {junk.cmpRes}");
                    context.MoveMouseAndClick(junk);
                    Thread.Sleep(1000);
                    var btn = buttons.ProcessingWithRetryTop1();
                    if (btn != null && btn.decision == "true")
                    {
                        context.MoveMouseAndClick(btn);
                        Thread.Sleep(1000);
                        int found = 0;
                        for (int i = 0; i < 30; i+= SLEEPS)
                        {
                            btn = buttons.ProcessingWithRetryTop1();
                            if (btn != null && btn.decision == "true" && btn.extraInfo.Contains("cancelremove"))
                            {
                                found++;
                            }
                            else
                                break;
                        }
                        if (found == 0)
                        {
                            context.DebugLog($"Wait not found, sleep 15s");
                            Thread.Sleep(1000 * 15);                           
                        }
                    }
                }
            }
        }
    }
}
