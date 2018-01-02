using ccInfo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class AutoResourceLoader
    {
        protected string _imgName;
        protected IHaveLog context;
        protected int topX;
        public AutoResourceLoader(IHaveLog ctx, string imgName, string dataDir, int top = 1, decimal threadHoldScala = (decimal)0.09) {            
            var files = Directory.GetFiles(dataDir);
            Init(ctx, imgName, files, top, threadHoldScala);
        }
        public AutoResourceLoader(IHaveLog ctx, string imgName, string[] files, int top = 1, decimal threadHoldScala = (decimal)0.09)
        {
            Init(ctx, imgName, files, top, threadHoldScala);
        }
        public void Init(IHaveLog ctx, string imgName, string[] files, int top = 1, decimal threadHoldScala = (decimal)0.09)
        {
            _imgName = imgName;
            context = ctx;
            topX = top;            
            foreach (var f in files)
            {
                if (f.EndsWith(".png"))
                {
                    clicks.Add(new ImgChecksAndTags(f, threadHoldScala));
                }
            }
        }

        static List<CommandInfo> empty = new List<CommandInfo>();
        public List<CommandInfo> ProcessingWithRetry(Func<List<CommandInfo>,bool> okfunc = null, int retryCount = 3,
            Func<ImgChecksAndTags, bool> tagCheck = null, bool doCenter = true)
        {
            if (okfunc == null) okfunc = res => res.Count > 0 && res.Any(r=>r.decision == "true");
            for (int i = 0; i < retryCount; i++)
            {
                var cmds = Processing(tagCheck, doCenter);
                if (!cmds.Any() || !okfunc(cmds))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                return cmds;
            }
            empty.Clear();
            return empty;
        }

        public CommandInfo ProcessingWithRetryTop1(Func<List<CommandInfo>, bool> okfunc = null, int retryCount = 3,
            Func<ImgChecksAndTags, bool> tagCheck = null, bool doCenter = true)
        {

            var cmds = ProcessingWithRetry(okfunc, retryCount, tagCheck, doCenter);
            return cmds.FirstOrDefault();
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
