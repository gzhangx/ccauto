using ccInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class ActionStructureNameReco
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

        static public string[] GetGoodNames()
        {
            return new[]
            {
                GoldStorage,
                ElixirStorage,
            };
        }
        public static string tempImgName = StandardClicks.GetTempDirFile("structureName.png");
        

        public static List<CommandInfo> GetNameAtPoint(ProcessingContext context, ccPoint loc)
        {
            context.MoveMouseAndClick(loc);
            context.Sleep(1000);
            Utils.doScreenShoot(tempImgName);
            return context.GetAppInfo();            
        }
        public static NameLevel GetStructureName(ccPoint loc, List<CommandInfo> results, IHaveLog context)
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
                context.InfoLog($" at {loc.x},{loc.y} got {bottom.command}:{bottom.Text} diff {bestDiff}");
                if (bestDiff < 3)
                {
                    {
                        var fullTxt = bottom.Text;
                        context.InfoLog("BESTTAG====> " + bestTag + " " + best);
                        int levelInd = fullTxt.IndexOf("level") + 5;
                        int endInd = fullTxt.IndexOf(")", levelInd);
                        string level = "";
                        if (levelInd > 0 && endInd > levelInd)
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

    }
}
