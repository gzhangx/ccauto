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
        const string GoldMine = "GoldMine";
        const string ElixirCollector = "ElixirCollector";
        const string TownHall = "TownHall";
        const string GoldStorage = "GoldStorage";
        const string ElixirStorage = "ElixirStorage";
        const string Barracks = "Barracks";
        const string ArmyCamp = "ArmyCamp";
        const string Laboratory = "Laboratory";
        const string ClanCastle = "ClanCastle";
        private ProcessingContext context;
        DateTime lastProcessDate = DateTime.Now.AddMinutes(-1000);

        public const string tempImgName = "tstimgs\\tempFullScreenAct.png";        

        public ProcessorMapByText(ProcessingContext ctx)
        {
            context = ctx;
        }
        List<PosInfo> locations = new List<PosInfo>();
        public void ProcessCommand(int act)
        {
            //Test();
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


            var badLocs = new List<PosInfo>();
            int nameTry = 0;
            var results = Utils.GetAppInfo();
            int numBuilders = NumBuilders(results);
            bool gotFirstGold = false;
            bool gotFirstEli = false;
            bool trained = false;
            foreach (var loc in locations)
            {
                if (numBuilders == 0 && !string.IsNullOrWhiteSpace(loc.name))
                {
                    if (loc.name == GoldMine && !gotFirstGold)
                    {
                        gotFirstGold = true;
                        context.MoveMouseAndClick(loc.point.x, loc.point.y);
                    }
                    if (loc.name == ElixirCollector && !gotFirstEli)
                    {
                        gotFirstEli = true;
                        context.MoveMouseAndClick(loc.point.x, loc.point.y);
                    }
                    if (loc.name != TownHall && loc.name != Barracks) continue;
                }
                context.MoveMouseAndClick(loc.point.x, loc.point.y);
                Thread.Sleep(1000);
                Utils.doScreenShoot(tempImgName);
                results = Utils.GetAppInfo();
                if (string.IsNullOrWhiteSpace(loc.name))
                {
                    loc.name = GetStructureName(loc, results);
                    context.DebugLog($"     FOUND {loc.name}");
                    if (string.IsNullOrWhiteSpace(loc.name))
                    {
                        nameTry++;
                        context.MoveMouseAndClick(loc.point.x, loc.point.y);
                        Thread.Sleep(1000);
                        results = Utils.GetAppInfo();
                        loc.name = GetStructureName(loc, results);
                        if (string.IsNullOrWhiteSpace(loc.name))
                        {
                            badLocs.Add(loc);
                            context.DebugLog("     Removing bad loc");
                        }
                    }
                }
                //"RecoResult_INFO_Builders"
                numBuilders = NumBuilders(results);
                context.InfoLog($"Number of builders available {numBuilders}");
                if (numBuilders == 0)
                {
                    if (loc.name != TownHall && loc.name != Barracks) continue;
                    if (loc.name == Barracks && trained) continue;
                }
                var actionItems = canUpgrade(tempImgName);
                if (numBuilders > 0)
                {
                    RetryAction(actionItems.upgrade, Upgraded);                    
                }
                foreach (var otherAct in actionItems.other)
                {
                    switch (otherAct.extraInfo)
                    {
                        case "RearmAll":
                            RetryAction(otherAct, () => CheckMatchAndAct("okcancel.png 3000 ", 300, 54));
                            break;
                        case "Train":
                            if (!trained)
                            {
                                RetryAction(otherAct, () => CheckMatchAndAct("buildwizardbutton.png 30 ", 54, 46, 10));
                                trained = true;
                            }
                            break;
                    }
                }
            }
            foreach (var p in badLocs) locations.Remove(p);
            File.WriteAllText(fname, JsonConvert.SerializeObject(locations, Formatting.Indented));
        }

        private bool RetryAction(CommandInfo cmd, Func<bool> act)
        {
            if (cmd != null)
            {
                context.MoveMouseAndClick(cmd.x + 20, cmd.y + 20);
                bool gotit = false;
                for (int retry = 0; retry < 3; retry++)
                {
                    if (act())
                    {
                        gotit = true;
                        break;
                    }
                }
                var results = Utils.GetAppInfo();
                context.DoStdClicks(results);
                return gotit;
            }
            return false;
        }

        private bool Upgraded()
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
                return true;
            }
            return false;
        }

        private bool CheckMatchAndAct(string img, int offx, int offy, int repeat = 1)
        {
            Utils.doScreenShoot(tempImgName);
            var sb = new StringBuilder();
            sb.Append($"-input {tempImgName} ");
            sb.Append($"-name g1 -match data\\check\\{img} ");
            var btns = Utils.GetAppInfo(sb.ToString());
            foreach (var btn in btns) context.DebugLog("           check rearmall " + btn);
            btns = btns.Where(r => r.decision == "true").OrderBy(r => r.cmpRes).ToList();
            if (btns.FirstOrDefault() != null)
            {
                var btn = btns.First();
                while (repeat > 0)
                {
                    context.MoveMouseAndClick(btn.x + offx, btn.y + offy);
                    repeat--;
                    if (repeat != 0)
                    {
                        btns = Utils.GetAppInfo(sb.ToString());
                        btn = btns.FirstOrDefault(r => r.decision == "true");
                        if (btn == null) break;
                    }
                }
                return true;
            }
            return false;
        }

        private void Test()
        {
            var results = Utils.GetAppInfo();
            //"RecoResult_INFO_Builders"
            //int num = NumBuilders(results);
            //context.InfoLog("Number of builders " + num);
            var res = GetStructureName(new PosInfo(), results);
            Console.WriteLine(res);
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
            string[] tags = new string[] {
            GoldMine, ElixirCollector, TownHall, GoldStorage, ElixirStorage,
            Barracks, ArmyCamp, Laboratory,
            ClanCastle,
            };
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
            public List<CommandInfo> other;
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
            var otherActs = new[] { "Train", "RearmAll" };
            var otherImgs = new Dictionary<string, string>
            {
                {"Train","data\\check\\traintroops.png 30" },
                {"RearmAll","data\\check\\rearmall.png 30" }
            };
            foreach (var name in otherActs)
                sb.Append($" -name {name} -matchRect {actx},{acty},650,105_200 -match {otherImgs[name]} ");            
            var res = Utils.GetAppInfo(sb.ToString());            
            res.ForEach(r =>
            {
                r.x += actx;
                r.y += acty;
            });
            foreach (var r in res) Console.WriteLine("           DEBUGRM " + r);
            res = res.Where(r => r.decision == "true").OrderBy(r => r.cmpRes).ToList();
            var others = new List<CommandInfo>();
            foreach (var name in otherActs)
            {
                var found = res.FirstOrDefault(r => r.extraInfo == name);
                if (found != null) others.Add(found);
            }
            return new UpgradeTrain
            {
                upgrade = res.FirstOrDefault(r => r.extraInfo == "Good" || r.extraInfo == "Bad"),
                other = others,
            };
        }
    }
}
