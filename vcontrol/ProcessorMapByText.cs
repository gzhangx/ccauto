using ccInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ccVcontrol
{

    public class ProcessorMapByText
    {
        const string GoldMine = "GoldMine(Level)";
        const string ElixirCollector = "ElixirCollector(level)";
        const string TownHall = "TownHall(level)";
        const string GoldStorage = "GoldStorage(level)";
        const string ElixirStorage = "ElixirStorage(level)";
        private ProcessingContext context;
        DateTime lastProcessDate = DateTime.Now.AddMinutes(-1000);

        const string tempImgName = "tstimgs\\tempFullScreenAct.png";

        string[] tags = new string[] { GoldMine, ElixirCollector, TownHall, GoldStorage, ElixirStorage };

        public ProcessorMapByText(ProcessingContext ctx)
        {
            context = ctx;
        }
        List<PosInfo> locations = new List<PosInfo>();
        public void ProcessCommand(int act)
        {
            if (act <= 0)
            {
                context.InfoLog("Failed to recognize account");
                return;
            }
            var fname = $"data\\accounts\\accountFull_{act}.txt";
            if (File.Exists(fname))
            {
                locations = JsonConvert.DeserializeObject<List<PosInfo>>(File.ReadAllText(fname));
            }
            locations.ForEach(l =>
            {
                Console.WriteLine("====>" + l.name + " " + l.point.x + "," + l.point.y);
            });
            if ((DateTime.Now - lastProcessDate).TotalMinutes > 40)
            {
                lastProcessDate = DateTime.Now;
            }
            else return;


            foreach (var loc in locations)
            {
                context.MoveMouseAndClick(loc.point.x, loc.point.y);
                Thread.Sleep(1000);
                Utils.doScreenShoot(tempImgName);
                var results = Utils.GetAppInfo();
                //"RecoResult_INFO_Builders"
                int numBuilders = NumBuilders(results);
                context.InfoLog($"Number of builders available {numBuilders}");
                var actionItems = canUpgrade(tempImgName);
                if (numBuilders > 0)
                {
                    if (actionItems.upgrade != null)
                    {
                        context.MoveMouseAndClick(actionItems.upgrade.x + 20, actionItems.upgrade.y + 20);
                        for (int retry = 0; retry < 3; retry++)
                        {
                            Utils.doScreenShoot(tempImgName);
                            var sb = new StringBuilder();
                            sb.Append($"-input {tempImgName} ");                            
                            sb.Append($"-name g1 -match data\\check\\upgradeWithEliButton.png 400 ");                            
                            sb.Append($" -name g2 -match data\\check\\upgradeWithGoldButton.png 400 ");
                            var btns = Utils.GetAppInfo(sb.ToString());
                            foreach (var btn in btns) context.DebugLog("           check train button " + btn);
                            btns = btns.Where(r => r.decision == "true").OrderBy(r => r.cmpRes).ToList();
                            if (btns.FirstOrDefault() != null)
                            {
                                var btn = btns.First();
                                context.MoveMouseAndClick(btn.x + 20, btn.y + 20);
                                break;
                            }
                        }
                        results = Utils.GetAppInfo();
                        context.DoStdClicks(results);
                    }
                }
                //ExtractItemname(loc);
            }
        }

        private void ExtractItemname(PosInfo loc)
        {
            var results = Utils.GetAppInfo();
            //"RecoResult_INFO_Builders"
            int num = NumBuilders(results);
            context.InfoLog("Number of builders " + num);
            GetStructureName(loc, results);
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

        private string GetStructureName(PosInfo loc, List<CommandInfo> results)
        {
            var bottom = results.FirstOrDefault(r => r.command == "RecoResult_INFO_Bottom");
            string best = "";
            string bestTag = "";
            if (bottom != null)
            {
                foreach (var tag in tags)
                {
                    var res = LCS.LongestCommonSubsequence(tag.ToLower(), bottom.Text.ToLower());
                    if (res.Length > best.Length)
                    {
                        best = res;
                        bestTag = tag;
                    }
                }
                int bestDiff = bestTag.Length - best.Length;
                Console.WriteLine($" at {loc.point.x},{loc.point.y} got {bottom.command}:{bottom.Text} diff {bestDiff}");
                if (bestDiff < 2)
                {
                    {
                        Console.WriteLine("BESTTAG====> " + bestTag + " " + best);
                        return bestTag;
                    }
                }
            }
            return null;
        }

        public class UpgradeTrain
        {
            public CommandInfo upgrade;
            public CommandInfo train;
            public CommandInfo rearm;
        }
        public static UpgradeTrain canUpgrade(string imgName)
        {
            string[] resultTypes = new[] { "Good", "Bad" };
            string[] itemTypes = new[] { "Gold", "Eli" };
            var sb = new StringBuilder();
            sb.Append($"-input {imgName} ");
            const int actx = 200;
            const int acty = 630;
            foreach (var rt in resultTypes)
            {
                foreach (var itm in itemTypes)
                {
                    sb.Append($"-name {rt} -matchRect {actx},{acty},650,105_200 -match data\\check\\upgrade{itm}{rt}.png 40 ");
                }
            }
            sb.Append($" -name Train -matchRect {actx},{acty},650,105_200 -match data\\check\\traintroops.png 40 ");
            sb.Append($" -name RearmAll -matchRect {actx},{acty},650,105_200 -match data\\check\\rearmall.png 30 ");
            var res = Utils.GetAppInfo(sb.ToString());
            res = res.Where(r => r.decision == "true").OrderBy(r => r.cmpRes).ToList();
            res.ForEach(r =>
            {
                r.x += actx;
                r.y += acty;
            });
            foreach (var r in res) Console.WriteLine("           DEBUGRM " + r);
            var found = res.FirstOrDefault();
            return new UpgradeTrain
            {
                upgrade = res.FirstOrDefault(r => r.extraInfo == "Good" || r.extraInfo == "Bad"),
                train = res.FirstOrDefault(r => r.extraInfo == "Train"),
                rearm = res.FirstOrDefault(r=>r.extraInfo == "RearmAll"),
            };
        }
    }
}
