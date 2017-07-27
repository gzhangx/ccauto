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

            public Action<CommandInfo, StepInfo> Act;
        }

        protected ProcessingContext context;
        public BaseProcessor(ProcessingContext ctx)
        {
            context = ctx;
        }


        public int DoSteps(List<StepInfo> steps)
        {
            int step = 0;
            var stepRetry = new int[steps.Count];
            while (true)
            {
                for (int i = step; i < steps.Count; i++)
                {
                    var cur = steps[i];
                    var fullInputPath = $"{imgdir}\\{cur.inputName}";
                    var fullcmd = $"-input {fullInputPath} {cur.cmd}";
                    context.DebugLog($"Doing step {cur.name} {fullcmd}");
                    Utils.doScreenShoot(fullInputPath);
                    var found = FindSpot(fullcmd, 1);
                    if (found == null)
                    {
                        if (i == step) stepRetry[i]++;
                    }
                    else
                    {
                        step = i + 1;
                        if (cur.Act != null)
                            cur.Act(found, cur);
                        else
                        {
                            context.MoveMouseAndClick(found.x + cur.xoff, found.y + cur.yoff);
                            context.MouseMouseTo(0, 0);
                            Thread.Sleep(cur.delay);
                        }
                    }
                }
                if (step >= steps.Count) break;
                for (int i = 0; i < stepRetry.Length; i++)
                {
                    var cur = steps[i];
                    if (stepRetry[i] > cur.maxRetry)
                    {
                        context.DebugLog($"Doing Timeout step {cur.name} {cur.cmd} {stepRetry[i]}/{cur.maxRetry}");
                        return step;
                    }
                }
            }
            return step;
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
