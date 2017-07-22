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
        public class StepInfo
        {
            public string name;
            public string cmd;
            public int xoff;
            public int yoff;
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
                    Console.WriteLine($"Doing step {cur.name} {cur.cmd}");
                    var found = FindSpot(cur.cmd, 1);
                    if (found == null)
                    {
                        stepRetry[i]++;
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
                        }
                    }
                }
                if (step >= steps.Count) break;
                for (int i = 0; i < stepRetry.Length; i++)
                {
                    var cur = steps[i];
                    if (stepRetry[i] > cur.maxRetry)
                    {
                        Console.WriteLine($"Doing Timeout step {cur.name} {cur.cmd} {stepRetry[i]}/{cur.maxRetry}");
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
                Console.WriteLine("Trying to find SINGLEMATCH for " + name);
                var cmds = Utils.GetAppInfo(name);
                var found = cmds.FirstOrDefault(cmd => cmd.command == "SINGLEMATCH");
                if (found != null)
                {
                    Console.WriteLine($"matching {name} found {found.cmpRes}");
                    return found;
                }
                Thread.Sleep(1000);
            }
            return null;
        }
        
        public abstract void Process();
    }
}
