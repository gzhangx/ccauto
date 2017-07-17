using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class ProcessorDonation
    {
        private ProcessingContext context;
        DateTime lastProcessDate = DateTime.Now;
        public ProcessorDonation(ProcessingContext ctx)
        {
            context = ctx;
        }
        public void ProcessDonate( CommandInfo cmd)
        {
            if ((DateTime.Now - lastProcessDate).TotalMinutes > 1)
            {
                lastProcessDate = DateTime.Now;
            }
            else return;
            Utils.MouseMouseTo(context.mouse, cmd.x, cmd.y);
            Utils.MouseClick(context.mouse);
        }
    }
}
