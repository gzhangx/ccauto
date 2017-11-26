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

        public class ImgChecksAndTags
        {
            public string ImageName { get; private set; }
            public ccPoint point;
            public decimal Threadshold;
            public ImgChecksAndTags(string imgName, decimal threadshold = 200000)
            {
                ImageName = imgName;
                Threadshold = threadshold;
                {
                    var img = new Bitmap(ImageName);
                    point = new ccPoint(img.Width / 2, img.Height / 2);
                }
            }
        }

        public List<CommandInfo> Processing()
        {
            Utils.doScreenShoot(_imgName);
            var sb = new StringBuilder();
            sb.Append($"-input {_imgName} -top {topX} ");
            foreach (var clk in clicks)
            {
                sb.Append($"-name {clk.ImageName} -match {clk.ImageName} {clk.Threadshold} ");
            }
            var results = Utils.GetAppInfo(sb.ToString(), context);
            return results;
        }
        private List<ImgChecksAndTags> clicks = new List<ImgChecksAndTags>();
    }
}
