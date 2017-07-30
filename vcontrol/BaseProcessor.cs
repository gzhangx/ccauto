using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public abstract class BaseProcessor
    {
        const string imgdir = "tstimgs";
        public class StepInfo
        {
            public string name;
            public string inputName;
            public string cmd;
            public int xoff;
            public int yoff;
            public int delay = 100;
            public int maxRetry = 10; //10s

            public Action<CommandInfo, StepInfo, StepContext> Act;
        }

        public class StepContext
        {
            public int step;
            public int[] stepRetry;
            public bool finished = false;
            public bool failed = false;
            public bool finishedGood = false;
        }

        protected ProcessingContext context;
        public BaseProcessor(ProcessingContext ctx)
        {
            context = ctx;
        }


        public StepContext DoSteps(List<StepInfo> steps)
        {
            var stepCtx = new StepContext
            {
                step = 0,
                stepRetry = new int[steps.Count]
            };
            
            while (true)
            {
                for (int i = stepCtx.step; i < steps.Count; i++)
                {
                    var cur = steps[i];
                    var fullInputPath = $"{imgdir}\\{cur.inputName}";
                    var fullcmd = $"-input {fullInputPath} {cur.cmd.Replace("-match ", "-match data\\check\\")}";
                    context.DebugLog($"Doing step {cur.name} {fullcmd}");
                    Utils.doScreenShoot(fullInputPath);
                    var found = FindSpot(fullcmd, 1);
                    if (found == null)
                    {
                        if (i == stepCtx.step) stepCtx.stepRetry[i]++;
                    }
                    else
                    {
                        stepCtx.step = i + 1;
                        if (cur.Act != null)
                        {
                            cur.Act(found, cur, stepCtx);
                            if (stepCtx.finished) break;
                        }
                        else
                        {
                            context.MoveMouseAndClick(found.x + cur.xoff, found.y + cur.yoff);
                            context.MouseMouseTo(0, 0);
                            Thread.Sleep(cur.delay);
                        }
                    }
                }
                if (stepCtx.step >= steps.Count || stepCtx.finished) break;
                for (int i = 0; i < stepCtx.stepRetry.Length; i++)
                {
                    var cur = steps[i];
                    if (stepCtx.stepRetry[i] > cur.maxRetry)
                    {
                        context.DebugLog($"Doing Timeout step {cur.name} {cur.cmd} {stepCtx.stepRetry[i]}/{cur.maxRetry}");
                        return stepCtx;
                    }
                }
            }
            return stepCtx;
        }

        public CommandInfo FindSpot(string name, int retry = 5)
        {
            for (int retryi = 0; retryi < retry; retryi++)
            {
                context.DebugLog("Trying to find SINGLEMATCH for " + name);
                var cmds = Utils.GetAppInfo(name);
                var found = cmds.FirstOrDefault(cmd => cmd.command == "SINGLEMATCH");
                if (found != null)
                {
                    context.InfoLog($"matching {name} found {found.cmpRes}");
                    return found;
                }
                Thread.Sleep(1000);
            }
            return null;
        }

        public void InitGame(string name)
        {
            var cmds = Utils.GetAppInfo();            
            while (true)
            {
                context.DoStdClicks(cmds);
                cmds = Utils.GetAppInfo();
                if (cmds.FirstOrDefault(c => c.command == "PRMXYCLICK_ACT_LeftExpand") != null) break;
            }
            Thread.Sleep(1000);
            //Utils.GetAppInfo($"-name {name} -matchRect 79,32,167,22_200 -screenshoot");
        }
        
        public abstract void Process();
    }
}
