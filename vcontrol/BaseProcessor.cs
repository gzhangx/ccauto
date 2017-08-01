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

            public int stepInd;
            public string[] otherStepCheck;
            public override string ToString()
            {
                return $"StepInfo {name} inpu={inputName} {cmd} {xoff},{yoff}";
            }
        }

        public class StepContext
        {
            public int step;
            public int[] stepRetry;
            public bool finished = false;
            public bool failed = false;
            public bool finishedGood = false;

            public List<StepInfo> Steps;
        }

        protected ProcessingContext context;
        public BaseProcessor(ProcessingContext ctx)
        {
            context = ctx;
        }

        protected bool checkRetryFail(StepContext stepCtx, int i)
        {
            StepInfo cur = stepCtx.Steps[i];
            if (stepCtx.stepRetry[i] > cur.maxRetry)
            {
                stepCtx.failed = true;
                stepCtx.finished = true;
                return true;
            }
            return false;
        }

        protected bool FoundOtherGoodAlts(StepContext stepCtx, StepInfo cur)
        {
            if (cur.otherStepCheck != null)
            {
                bool otherFound = false;
                foreach (var other in cur.otherStepCheck)
                {
                    if (CheckStep(stepCtx, other))
                    {
                        otherFound = true;
                        break;
                    }
                }
                context.DebugLog($"   FOUND ALT {otherFound}");
                return (otherFound);
            }
            return false;
        }
        protected StepContext DoSteps(List<StepInfo> steps)
        {
            var stepCtx = new StepContext
            {
                step = 0,
                Steps = steps,
                stepRetry = new int[steps.Count]
            };
            for (int i = 0; i < steps.Count; i++) steps[i].stepInd = i;

            //while (!stepCtx.finished)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    var cur = steps[i];
                    stepCtx.step = i;
                    CommandInfo found = FindSpotOnStep(cur);
                    if (found == null)
                    {
                        stepCtx.stepRetry[i]++;
                        if (FoundOtherGoodAlts(stepCtx, cur))
                        {
                            i = stepCtx.step;
                            continue;
                        }
                        if (stepCtx.stepRetry[i] > cur.maxRetry)
                        {
                            stepCtx.failed = true;
                            stepCtx.finished = true;
                            break;
                        }
                        i--;
                    }
                    else
                    {
                        if (cur.Act != null)
                        {
                            cur.Act(found, cur, stepCtx);
                            i = stepCtx.step;                            
                            if (stepCtx.finished) break;
                        }
                        else
                        {
                            context.MoveMouseAndClick(found.x + cur.xoff, found.y + cur.yoff);
                            context.MouseMouseTo(0, 0);
                            bool lastStepRes = WaitNextStep(stepCtx);
                            if (!lastStepRes)
                            {
                                stepCtx.failed = true;
                                stepCtx.stepRetry[i]++;
                                if (checkRetryFail(stepCtx, i)) break;
                                i--;
                            }else
                            {
                                if (i == steps.Count - 1)
                                {
                                    stepCtx.finished = true;                                    
                                }
                            }
                        }
                    }
                }
            }            
            return stepCtx;
        }


        protected bool CheckStep(StepContext stepCtx, string stepName)
        {
            var step = stepCtx.Steps.FirstOrDefault(s => s.name == stepName);

            if (FindSpotOnStep(step, $"   CHECKSEP {stepName} ") != null)
            {
                stepCtx.step = step.stepInd - 1;
                return true;
            }

            return false;
        }

        protected bool WaitNextStep(StepContext stepCtx)
        {
            var cur = stepCtx.Steps[stepCtx.step];
            var nextStepPt = stepCtx.step + 1;
            if (nextStepPt >= stepCtx.Steps.Count)
            {
                if  (cur.delay > 0)
                {
                    Thread.Sleep(cur.delay);
                }
                return true;
            }
            else
            {
                for (int wt = 0; wt < cur.delay; wt += 1000)
                {
                    var nextStep = stepCtx.Steps[nextStepPt];
                    if (FindSpotOnStep(nextStep, "   WAITNEXT ") != null)
                    {
                        return true;
                    }
                    if (FoundOtherGoodAlts(stepCtx, nextStep)) return true;
                    Thread.Sleep(1000);
                }
            }

            return false;
        }

        protected virtual CommandInfo FindSpotOnStep(StepInfo cur, string printDebug= "")
        {
            var fullInputPath = $"{imgdir}\\{cur.inputName}";
            var fullcmd = $"-input {fullInputPath} {cur.cmd.Replace("-match ", "-match data\\check\\")}";
            context.DebugLog($"{printDebug}Doing step {cur.name} {fullcmd}");
            Utils.doScreenShoot(fullInputPath);
            var found = FindSpot(fullcmd, 1, printDebug);
            return found;
        }

        public CommandInfo FindSpot(string name, int retry = 5, string printDebug = "")
        {
            for (int retryi = 0; retryi < retry; retryi++)
            {
                context.DebugLog($"{printDebug}Trying to find SINGLEMATCH for {name}");
                var cmds = Utils.GetAppInfo(name);
                var found = cmds.FirstOrDefault(cmd => cmd.command == "SINGLEMATCH");
                if (found != null && found.decision == "true")
                {
                    context.InfoLog($"{printDebug}matching {name} found {found.cmpRes} {found.decision}");
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
        
        public abstract StepContext Process();
    }
}
