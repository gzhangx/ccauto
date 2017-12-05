using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class ActionUpgrade
    {
        private ProcessingContext context;
        public static string tempImgName = StandardClicks.GetTempDirFile("tempTrainUnits.png");
        private AutoResourceLoader upgradeBuildingFinder;
        private ActionStructureNameReco namerec;
        public ActionUpgrade(ProcessingContext ctx)
        {
            context = ctx;
            upgradeBuildingFinder = new AutoResourceLoader(ctx, tempImgName,
                @"data\check\action\buildings\autocheck");
        }
        public void Process()
        {
            var items = upgradeBuildingFinder.Processing();
            foreach (var itm in items)
            {
                if (itm.x > 200)
                {
                    context.InfoLog($"found {itm.extraInfo} {itm.x}/{itm.y}");
                    var cmdres = ActionStructureNameReco.GetNameAtPoint(context, itm);
                    var nameLevel = ActionStructureNameReco.GetStructureName(itm, cmdres, context);
                    var goodNames = ActionStructureNameReco.GetGoodNames();
                    var numBuilders = NumBuilders(cmdres);
                    if (numBuilders == 0)
                    {
                        context.InfoLog($"No builder");
                        return;
                    }
                    foreach (var gname in goodNames)
                    {
                        if (gname.ToLower() == nameLevel.name.ToLower())
                        {
                            context.DebugLog($"found match name {gname}");
                            //context.MoveMouseAndClick(itm);
                            //Thread.Sleep(1000);
                            var upgGrood = canUpgrade(context, tempImgName, numBuilders);
                            if (upgGrood != null && upgGrood.extraInfo.Contains("Good"))
                            {
                                Upgraded();
                                numBuilders--;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private int NumBuilders(List<CommandInfo> cmds)
        {
            var cmd = cmds.FirstOrDefault(c => c.command == "RecoResult_INFO_Builders");
            try
            {
                if (cmd != null)
                {
                    if (cmd.Text != null && cmd.Text.Contains("/"))
                    {
                        var cc = cmd.Text.Split('/');
                        return int.Parse(cc[0].Trim());
                    }
                }
            }
            catch (Exception exc)
            {
                context.InfoLog("ERROR " + exc.ToString() + " " + cmd);
            }
            return 0;
        }

        private bool Upgraded()
        {
            Utils.doScreenShoot(tempImgName);
            var sb = new StringBuilder();
            sb.Append($"-input {tempImgName} ");
            sb.Append($"-name g1 -match data\\check\\upgradeWithEliButton.png 400 ");
            sb.Append($" -name g2 -match data\\check\\upgradeWithGoldButton.png 400 ");
            var btns = context.GetAppInfo(sb.ToString());
            foreach (var btn in btns) context.DebugLog("           check train button " + btn);
            btns = btns.Where(r => r.decision == "true").OrderBy(r => r.cmpRes).ToList();
            if (btns.FirstOrDefault() != null)
            {
                var btn = btns.First();
                var fname = btn.extraInfo == "g1" ? "upgradeWithEliButton.png" : "upgradeWithGoldButton.png";
                context.LogMatchAnalyst($"UPGRADING -match {fname} 400", btn.cmpRes);
                context.MoveMouseAndClick(btn.x + 20, btn.y + 20);
                return true;
            }
            return false;
        }

        public static CommandInfo canUpgrade(ProcessingContext context, string imgName, int numBuilders)
        {
            string[] resultTypes = new[] { "Good", "Bad" };
            string[] itemTypes = new[] { "Gold", "Eli" };
            var sb = new StringBuilder();
            sb.Append($"-input {imgName} ");
            const int actx = 200;
            const int acty = 630;
            if (numBuilders > 0)
            {
                foreach (var rt in resultTypes)
                {
                    foreach (var itm in itemTypes)
                    {
                        sb.Append($"-name {rt} -matchRect {actx},{acty},650,105_200 -match data\\check\\upgrade{itm}{rt}.png 40 ");
                    }
                }
            }
            var res = context.GetAppInfo(sb.ToString());
            res.ForEach(r =>
            {
                r.x += actx;
                r.y += acty;
            });

            if (context != null)
                foreach (var r in res) context.DebugLog("           DEBUGRM " + r);
            res = res.Where(r => r.decision == "true").OrderBy(r => r.cmpRes).ToList();

            if (context != null)
            {
                var upgrade = res.FirstOrDefault(r => r.extraInfo == "Good" || r.extraInfo == "Bad");
                if (upgrade != null)
                    context.LogMatchAnalyst(sb.ToString(), upgrade.cmpRes);
            }
            var others = new List<CommandInfo>();
            return res.FirstOrDefault(r => r.extraInfo == "Good" || r.extraInfo == "Bad");            
        }
    }
}
