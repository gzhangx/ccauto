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
        public static string tempImgName = StandardClicks.tempImgName;
        public const string dataDir = "data\\check\\action\\res\\";
        static void tResourceClicks()
        {
            if (clicks.Count == 0)
            {
                var files = Directory.GetFiles(dataDir);
                foreach (var f in files)
                {
                    if (f.EndsWith(".png"))
                    {
                        clicks.Add(new ImgChecksAndTags(f));
                    }
                }
            }
        }
        public ResourceClicks(IHaveLog ctx)
        {
            tResourceClicks();
            context = ctx;
        }
        public List<CommandInfo> Processing()
        {
            Utils.doScreenShoot(tempImgName);
            var sb = new StringBuilder();
            sb.Append($"-input {tempImgName} -top 100 ");
            foreach(var clk in clicks)
            {
                sb.Append($"-name {clk.ImageName} -match {clk.ImageName} {clk.Threadshold} ");
            }
            var results = Utils.GetAppInfo(sb.ToString(), context);            
            return results;
        }
        public class ImgChecksAndTags
        {
            public string ImageName { get; private set;}
            public ccPoint point;
            public decimal Threadshold;
            public ImgChecksAndTags(string imgName,  decimal threadshold = 200000)
            {
                ImageName = imgName;
                Threadshold = threadshold;                
                {
                    var img = new Bitmap(ImageName);
                    point = new ccPoint(img.Width / 2, img.Height / 2);
                }
            }
        }
        static List<ImgChecksAndTags> clicks = new List<ImgChecksAndTags>();
    }
}
