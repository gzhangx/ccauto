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
    public class AutoResourceLoader
    {
        protected string _imgName;
        protected IHaveLog context;
        protected int topX;
        public AutoResourceLoader(IHaveLog ctx, string imgName, string dataDir, int top = 1) {
            _imgName = imgName;
            context = ctx;
            topX = top;
            //var tempImgName = StandardClicks.tempImgName;
            //var dataDir = "data\\check\\action\\res\\";
            var files = Directory.GetFiles(dataDir);
            foreach (var f in files)
            {
                if (f.EndsWith(".png"))
                {
                    clicks.Add(new ImgChecksAndTags(f));
                }
            }
        }

        

        public List<CommandInfo> Processing(Func<ImgChecksAndTags,bool> tagCheck = null,bool doCenter = true)
        {
            Utils.doScreenShoot(_imgName);
            var sb = new StringBuilder();
            sb.Append($"-input {_imgName} -top {topX} ");
            foreach (var clk in clicks)
            {
                if (tagCheck != null)
                    if (!tagCheck(clk)) continue;
                sb.Append($"-name {clk.ImageName} -match {clk.ImageName} {clk.Threadshold} ");
            }
            var results = Utils.GetAppInfo(sb.ToString(), context);
            results.ForEach(r =>
            {
                r.imgInfo = clicks.FirstOrDefault(c => c.ImageName == r.extraInfo);
                if (r.imgInfo != null && doCenter)
                {
                    r.x = r.x + (r.imgInfo.center.x);
                    r.y = r.y + (r.imgInfo.center.y);
                }
            });
            return results.OrderBy(x => x.cmpRes).ToList();            
        }
        public List<ImgChecksAndTags> clicks = new List<ImgChecksAndTags>();
    }
}
