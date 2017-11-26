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
        IHaveLog context;
        AutoResourceLoader ldr;
        public ResourceClicks(IHaveLog ctx)
        {
            ldr = new ccVcontrol.AutoResourceLoader(ctx, StandardClicks.tempImgName, "data\\check\\action\\res\\");            
            context = ctx;
        }
        public List<CommandInfo> Processing()
        {
            return ldr.Processing();
        }
        
    }
}
