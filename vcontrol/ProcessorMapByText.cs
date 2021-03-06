﻿using ccInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

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

        public const string tempImgName = StandardClicks.tempImgName;        

        public ProcessorMapByText(ProcessingContext ctx)
        {
            context = ctx;
        }
        List<PosInfo> locations = new List<PosInfo>();

        protected static List<PosInfo> reorderLocation(List<PosInfo> locations, IVDController controller)
        {
            var results = new List<PosInfo>();
            var processed = new Dictionary<string, bool>();
            foreach (var l in locations)
            {
                if (string.IsNullOrEmpty(l.Name())) continue;
                if (processed.ContainsKey(l.Name())) continue;
                processed.Add(l.Name(), true);
                var allNames = locations.FindAll(me => me.Name() == l.Name()).OrderBy(me => me.Level()).ToList();
                controller.Log("debug", $"before dup {l.Name()} has {allNames.Count} items");
                var allDist = new List<PosInfo>();
                allNames.ForEach(me =>
                {
                    if (allDist.FirstOrDefault(x=>x.point.x  == me.point.x && x.point.y == me.point.y) == null)
                    {
                        allDist.Add(me);
                    }
                });
                results.AddRange(allDist);
            }
            return results;
        }
        public void ProcessCommand(int act)
        {
            return;
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
            locations.ForEach(loc =>
            {
                Console.WriteLine("====>" + loc.Name() + " " + loc.point.x + "," + loc.point.y);
                if (context.vdcontroller.redoStructureNames) loc.nameLevel = null;
                for (int nameRetry = 0; nameRetry < 2; nameRetry++)
                {
                    if (loc.nameLevel == null)
                    {
                        context.MoveMouseAndClick(loc.point.x, loc.point.y);
                        context.Sleep(1000);
                        Utils.doScreenShoot(tempImgName);
                        var res = context.GetAppInfo();
                        loc.nameLevel = GetStructureName(loc, res);
                        context.vdcontroller.Log("info","====> After reco" + loc.Name() + " level=" + loc.Level());
                    }
                    else break;
                }
            });



            locations = reorderLocation(locations, context.vdcontroller);


            var badLocs = new List<PosInfo>();
            var results = context.GetAppInfo();
            int numBuilders = NumBuilders(results);
            bool gotFirstGold = false;
            bool gotFirstEli = false;
            bool trained = false;
            foreach (var loc in locations)
            {
                if (context.vdcontroller.humanMode) break;
                if (numBuilders == 0 && !string.IsNullOrWhiteSpace(loc.Name()))
                {
                    if (loc.Name() == GoldMine && !gotFirstGold)
                    {
                        gotFirstGold = true;
                        context.MoveMouseAndClick(loc.point.x, loc.point.y);
                    }
                    if (loc.Name() == ElixirCollector && !gotFirstEli)
                    {
                        gotFirstEli = true;
                        context.MoveMouseAndClick(loc.point.x, loc.point.y);
                    }
                    if (loc.Name() != TownHall && loc.Name() != Barracks) continue;
                    if (loc.Name() == Barracks && trained) continue;
                }
                context.MoveMouseAndClick(loc.point.x, loc.point.y);
                context.Sleep(1000);
                Utils.doScreenShoot(tempImgName);
                results = context.GetAppInfo();
                //"RecoResult_INFO_Builders"
                numBuilders = context.vdcontroller.doUpgrades? NumBuilders(results) : 0;
                context.InfoLog($"Number of builders available {numBuilders}");
                if (numBuilders == 0 || !context.vdcontroller.doUpgrades)
                {
                    if (loc.Name() != TownHall && loc.Name() != Barracks) continue;
                    if (loc.Name() == Barracks && trained) continue;
                }
                var actionItems = canUpgrade(context, tempImgName, numBuilders);
                if (numBuilders > 0 && context.vdcontroller.doUpgrades)
                {
                    RetryAction(actionItems.upgrade, Upgraded);                    
                }
                foreach (var otherAct in actionItems.other)
                {
                    if (context.vdcontroller.humanMode) break;
                    switch (otherAct.extraInfo)
                    {
                        case "RearmAll":
                            RetryAction(otherAct, () => CheckMatchAndAct("okcancel.png 3000 ", 300, 54));
                            break;
                        case "Train":
                            if (!trained && context.vdcontroller.doDonate)
                            {
                                RetryAction(otherAct, () => CheckMatchAndAct("buildwizardbutton.png 120 ", 54, 46, 10));
                                RetryAction(otherAct, () => CheckMatchAndAct("buildArchierButton.png 120 ", 54, 46, 2));
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
                context.DoStdClicks();
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

        private bool CheckMatchAndAct(string img, int offx, int offy, int repeat = 1)
        {
            Utils.doScreenShoot(tempImgName);
            var sb = new StringBuilder();
            sb.Append($"-input {tempImgName} ");
            sb.Append($"-name g1 -match data\\check\\{img} ");
            var btns = context.GetAppInfo(sb.ToString());
            foreach (var btn in btns) context.DebugLog("           check rearmall " + btn);
            btns = btns.Where(r => r.decision == "true").OrderBy(r => r.cmpRes).ToList();
            if (btns.FirstOrDefault() != null)
            {
                var btn = btns.First();
                context.LogMatchAnalyst(sb.ToString(), btn.cmpRes);
                context.MoveMouseAndClick(btn.x + offx, btn.y + offy);
                while (repeat > 0)
                {
                    context.MouseClick();
                    repeat--;
                    if (repeat != 0)
                    {
                        //btns = Utils.GetAppInfo(sb.ToString());
                        //btn = btns.FirstOrDefault(r => r.decision == "true");
                        //if (btn == null) break;
                    }
                }
                return true;
            }
            return false;
        }

        private void Test()
        {
            var results = context.GetAppInfo();
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

        private NameLevel GetStructureName(PosInfo loc, List<CommandInfo> results)
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
                context.vdcontroller.Log("info",$" at {loc.point.x},{loc.point.y} got {bottom.command}:{bottom.Text} diff {bestDiff}");
                if (bestDiff < 2)
                {
                    {
                        var fullTxt = bottom.Text;
                        context.vdcontroller.Log("info", "BESTTAG====> " + bestTag + " " + best);
                        int levelInd = fullTxt.IndexOf("level") + 5;
                        int endInd = fullTxt.IndexOf(")", levelInd);
                        string level = "";
                        if (levelInd >0 && endInd > levelInd)
                        {
                            level = fullTxt.Substring(levelInd, endInd - levelInd);
                        }
                        return new NameLevel
                        {
                            name = bestTag,
                            level = level,
                        };                        
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
        public static UpgradeTrain canUpgrade(ProcessingContext context, string imgName, int numBuilders)
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
            var otherActs = new[] { "Train", "RearmAll" };
            var otherImgs = new Dictionary<string, string>
            {
                {"Train","data\\check\\traintroops.png 30" },
                {"RearmAll","data\\check\\rearmall.png 30" }
            };
            foreach (var name in otherActs)
                sb.Append($" -name {name} -matchRect {actx},{acty},650,105_200 -match {otherImgs[name]} ");            
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
            foreach (var name in otherActs)
            {
                var found = res.FirstOrDefault(r => r.extraInfo == name);
                if (found != null)
                {
                    others.Add(found);
                    context.LogMatchAnalyst(sb.ToString(), found.cmpRes);
                }
            }
            return new UpgradeTrain
            {
                upgrade = res.FirstOrDefault(r => r.extraInfo == "Good" || r.extraInfo == "Bad"),
                other = others,
            };
        }
    }
}
