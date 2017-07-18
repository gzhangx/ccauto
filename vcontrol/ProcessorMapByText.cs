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
        const int width = 680;
        const int height = 516;
        const int step = 30;
        private ProcessingContext context;
        DateTime lastProcessDate = DateTime.Now.AddMinutes(-2);
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
                Utils.MouseMouseTo(context.mouse, startx, starty + y);
                for (int x = 0; x < width; x += step)
                {
                    Utils.MouseMouseRelative(context.mouse, step, 0);
                    Utils.MouseClick(context.mouse);
                    Thread.Sleep(1000);
                    var results = Utils.GetAppInfo();
                    var bottom = results.FirstOrDefault(r => r.command == "RecoResult_INFO_Bottom");
                    if (bottom != null)
                    {
                        Console.WriteLine("got " + bottom.command+ ":"+bottom.Text);
                    }
                }
            }
            
        }
    }
}
