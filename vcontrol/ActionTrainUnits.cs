using ccInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ccVcontrol
{


    public class ActionTrainUnits
    {        
        private ProcessingContext context;        

        public static string tempImgName = StandardClicks.GetTempDirFile("tempTrainUnits.png");
        private AutoResourceLoader trainButtonFinder;
        private AutoResourceLoader buildArcherWizardButton;
        public ActionTrainUnits(ProcessingContext ctx)
        {
            context = ctx;
            trainButtonFinder = new AutoResourceLoader(ctx, tempImgName, new string[]
            {
                @"data\check\action\buttons\train.png",
                @"data\check\action\buttons\trainTroopsTab.png"
            });
            buildArcherWizardButton = new AutoResourceLoader(ctx, tempImgName, new string[]
            {
                @"data\check\buildwizardbutton.png",
                @"data\check\buildArchierButton.png",
            });
        }
        public void ProcessCommand(ProcessingContext context)
        {
            var cmd = trainButtonFinder.ProcessingWithRetryTop1();
            if (cmd == null)
            {
                context.DebugLog("Didn't find Train button");
                return;
            }            
            context.MoveMouseAndClick(cmd);
            Thread.Sleep(1000);

            cmd = trainButtonFinder.ProcessingWithRetryTop1();
            if (cmd == null)
            {
                context.DebugLog("Didn't find Train tab button");
                return;
            }
            context.MoveMouseAndClick(cmd);
            Thread.Sleep(1000);

            var cmds = buildArcherWizardButton.ProcessingWithRetry();
            if (!cmds.Any())
            {
                context.DebugLog("Didn't find troop button");
                return;
            }
            cmd = cmds.FirstOrDefault(c => c.extraInfo.Contains("buildwizardbutton"));
            if (cmd != null)
            {
                context.MoveMouseAndClick(cmd);
                for (int i = 0; i < 10; i++)
                    context.MouseClick();
            }
            cmd = cmds.FirstOrDefault(c => c.extraInfo.Contains("buildArchierButton"));
            if (cmd != null)
            {
                context.MoveMouseAndClick(cmd);
                context.MouseClick();
            }
        }
        
    }
}
