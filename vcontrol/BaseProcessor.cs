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
            public string cmd;
            public int xoff;
            public int yoff;
        }

        protected ProcessingContext context;
        public BaseProcessor(ProcessingContext ctx)
        {
            context = ctx;
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
                    return found;
                }
                Thread.Sleep(1000);
            }
            return null;
        }
        
        public abstract void Process();
    }
}
