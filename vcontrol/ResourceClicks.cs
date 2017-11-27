using ccInfo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class ResourceClicks
    {
        ProcessingContext context;
        AutoResourceLoader ldr;
        public ResourceClicks(ProcessingContext ctx)
        {
            ldr = new ccVcontrol.AutoResourceLoader(ctx, StandardClicks.tempImgName, "data\\check\\action\\res\\", 2);            
            context = ctx;
        }
        public List<CommandInfo> Processing()
        {
            var res = ldr.Processing();
            foreach (var clk in res)
            {
                var yoff = 0;
                if (clk.extraInfo.IndexOf("_drop") > 0) yoff = 10;
                context.MoveMouseAndClick(clk.x, clk.y + yoff);
            }
            return res;
        }
        
    }
}
