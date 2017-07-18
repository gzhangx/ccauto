using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{

    public class ProcessorMapByText
    {
        const int startx = 127;
        const int starty = 100;
        const int width = 600;
        const int height = 480;
        const int step = 40;
        private ProcessingContext context;
        DateTime lastProcessDate = DateTime.Now.AddMinutes(-2);

        string[] tags = new string[] { "GoldMine(Level)", "ElixirCollector(level)" , "TownHall(level)", "GoldStorage(level)", "ElixirStorage(level)" };


        public ProcessorMapByText(ProcessingContext ctx)
        {
            context = ctx;
        }
        public void ProcessCommand()
        {            
            if ((DateTime.Now - lastProcessDate).TotalMinutes > 1)
            {
                lastProcessDate = DateTime.Now;
            }
            else return;

            
            for (int y = 0; y < height; y += step)
            {
                context.MouseMouseTo(startx, starty + y);
                for (int x = 0; x < width; x += step)
                {
                    context.MouseMouseRelative( step, 0);
                    context.MouseClick();
                    Thread.Sleep(1000);
                    var results = Utils.GetAppInfo();
                    context.DoStdClicks(results);
                    var bottom = results.FirstOrDefault(r => r.command == "RecoResult_INFO_Bottom");
                    string best = "";
                    string bestTag = "";
                    if (bottom != null)
                    {
                        Console.WriteLine("got " + bottom.command+ ":"+bottom.Text);
                        foreach(var tag in tags)
                        {
                            var res = LCS.LongestCommonSubsequence(tag.ToLower(), bottom.Text.ToLower());
                            if (res.Length > best.Length)
                            {
                                best = res;
                                bestTag = tag;
                            }
                        }
                        if (best.Length > 5)
                        {
                            bool good = false;
                            if (bestTag == "TownHall(level)")
                            {
                                if (best.Contains("T") || best.Contains("H")) good = true;
                            }
                            else good = true;

                            if (good)
                                Console.WriteLine("got " + bestTag + " " + best);
                        }
                    }
                }
            }
            
        }
    }
}
