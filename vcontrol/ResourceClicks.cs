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
        public enum CurBaseType
        {
            PrimaryBase,
            SecondaryBase,
            Unknown
        }
        ProcessingContext context;
        static AutoResourceLoader ldr;

        static AutoResourceLoader baseMark;
        public ResourceClicks(ProcessingContext ctx)
        {
            if (ldr == null)
                ldr = new ccVcontrol.AutoResourceLoader(ctx, StandardClicks.tempImgName, "data\\check\\action\\res\\", 2);
            if (baseMark == null)
                baseMark = new ccVcontrol.AutoResourceLoader(ctx, StandardClicks.tempImgName, "data\\check\\marks\\", 1);
            context = ctx;
        }
        public List<CommandInfo> Processing(CurBaseType baseType)
        {
            string filterby = "";
            switch(baseType)
            {
                case CurBaseType.Unknown:
                    return new List<CommandInfo>();
                case CurBaseType.PrimaryBase:
                    filterby = "primary";
                    break;
                case CurBaseType.SecondaryBase:
                    filterby = "secondary";
                    break;
            }
            var res = ldr.Processing(clk=>
            {
                return clk.ImageName.Contains(filterby);
            });
            foreach (var clk in res)
            {
                var yoff = 0;
                if (clk.extraInfo.IndexOf("_drop") > 0) yoff = 10;
                if (clk.x < 150 && clk.y < 650)
                {
                    context.InfoLog($"Bad click for {clk.extraInfo} {clk.x}/{clk.y}");
                    continue;
                }
                context.MoveMouseAndClick(clk.x, clk.y + yoff);
            }
            return res;
        }

        public CurBaseType PrimaryOrSecondary(int state = 0)
        {
            for (int retry = 0; retry < 3; retry++)
            {
                var baseInfo = baseMark.Processing();
                if (baseInfo.Count == 0)
                {
                    context.InfoLog($"Can't find primary nor secondary retry {retry}, sleep 5s");
                    context.Sleep(5000);
                    continue;
                }
                var theBase = baseInfo.First();
                if (theBase.cmpRes > 100)
                {
                    context.InfoLog($"Can't find primary nor secondary retry {retry}, base {theBase.ToString()}, sleep 5s");
                }
                if (theBase.extraInfo.IndexOf("primary_base") > 0)
                {
                    context.InfoLog("in primary base");
                    return CurBaseType.PrimaryBase;
                }
                if (theBase.extraInfo.IndexOf("secondary_base") > 0)
                {
                    context.InfoLog("in secondary base");
                    return CurBaseType.SecondaryBase;
                }
                break;
            }
            return CurBaseType.Unknown;
        }

        public void MoveToPrimary()
        {            
            context.MouseDragTo(726, 89, 601, 168);
            context.MouseMouseTo(0, 0);
            context.Sleep(2000);            
            for (int retry = 0; retry < 3; retry++)
            {
                var marks = baseMark.Processing();
                if (marks.Count == 0)
                {
                    context.InfoLog($"Can't find primary nor secondary retry {retry}, sleep 5s");
                    context.Sleep(5000);
                    continue;
                }

                var secboat = marks.FirstOrDefault(m => m.extraInfo.Contains("secondary_boat.png"));
                if (secboat == null)
                {
                    context.InfoLog("failed find boat");
                    continue;
                }
                if (secboat.x < 250 || secboat.y > 700)
                {
                    context.InfoLog("this is primary boat");
                    break;
                }
                context.MoveMouseAndClick(secboat.x, secboat.y);
                break;
            }
        }


        public void MoveToSecondary()
        {
            context.MouseDragTo(177, 682, 280, 552);
            context.MouseMouseTo(0, 0);
            context.Sleep(2000);
            for (int retry = 0; retry < 3; retry++)
            {
                var marks = baseMark.Processing();
                if (marks.Count == 0)
                {
                    context.InfoLog($"Can't find primary nor secondary retry {retry}, sleep 5s");
                    context.Sleep(5000);
                    continue;
                }

                var secboat = marks.FirstOrDefault(m => m.extraInfo.Contains("primary_boat.png"));
                if (secboat == null)
                {
                    context.InfoLog("failed find boat");
                    continue;
                }
                if (secboat.x > 753 || secboat.y < 199)
                {
                    context.InfoLog("this is secondary boat");
                    break;
                }
                context.MoveMouseAndClick(secboat.x, secboat.y);
                break;
            }
        }

    }
}
