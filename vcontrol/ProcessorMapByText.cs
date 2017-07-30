using ccInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{

    public class ProcessorMapByText
    {
        const string GoldMine = "GoldMine(Level)";
        const string ElixirCollector = "ElixirCollector(level)";
        const string TownHall = "TownHall(level)";
        const string GoldStorage = "GoldStorage(level)";
        const string ElixirStorage = "ElixirStorage(level)";
        const int startx = 127;
        const int starty = 100;
        const int width = 600;
        const int height = 480;
        const int step = 60;
        private ProcessingContext context;
        DateTime lastProcessDate = DateTime.Now.AddMinutes(-1000);

        string[] tags = new string[] { GoldMine , ElixirCollector, TownHall, GoldStorage, ElixirStorage};

        public ProcessorMapByText(ProcessingContext ctx)
        {
            context = ctx;
        }
        List<PosInfo> locations = new List<PosInfo>();
        public void ProcessCommand(int act)
        {
            var fname = $"data\\account\\accountFull_{act}.txt";
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

            locations.Clear();
            var results = Utils.GetAppInfo();
            context.DoStdClicks(results);
            for (int y = 0; y < height - 100; y += step)
            {
                context.MouseMouseTo(startx, starty + y);
                for (int x = 0; x < width - 100; x += step)
                {
                    context.MouseMouseRelative( step, 0);
                    context.MouseClick();
                    Thread.Sleep(1000);
                    results = Utils.GetAppInfo();
                    context.DoStdClicks(results);
                    var bottom = results.FirstOrDefault(r => r.command == "RecoResult_INFO_Bottom");
                    string best = "";
                    string bestTag = "";
                    if (bottom != null)
                    {                        
                        foreach(var tag in tags)
                        {
                            var res = LCS.LongestCommonSubsequence(tag.ToLower(), bottom.Text.ToLower());
                            if (res.Length > best.Length)
                            {
                                best = res;
                                bestTag = tag;
                            }
                        }
                        int bestDiff = bestTag.Length - best.Length;
                        Console.WriteLine($" at {y},{x} got {bottom.command}:{bottom.Text} diff {bestDiff}");
                        if (bestDiff < 2)
                        {                            
                            {
                                Console.WriteLine("BESTTAG====> " + bestTag + " " + best);
                                locations.Add(new TagAndLocation { tag = bestTag, x = x, y = y, });
                            }
                        }
                    }
                }
            }
            
        }
    }
}
