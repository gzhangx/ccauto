using ccInfo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class StandardClicks
    {
        ProcessingContext context;
        public static string GetTempDirFile(string fname)
        {
            return $"tstimgs\\{fname}";
        }

        public const string tempImgName = "tstimgs\\tempFullScreenAct.png";
        public StandardClicks(ProcessingContext ctx)
        {
            context = ctx;
        }
        public List<CommandInfo> Processing()
        {
            Utils.doScreenShoot(tempImgName);
            var sb = new StringBuilder();
            sb.Append($"-input {tempImgName} ");
            foreach(var clk in clicks)
            {
                sb.Append($"-name {clk.Desc} -match {clk.ImageName} {clk.Threadshold} ");
            }
            var results = context.GetAppInfo(sb.ToString());
            var topMatch = results.Where(r => r.decision == "true" 
            && r.extraInfo != null 
            && r.extraInfo.StartsWith("PRMXYCLICK_STD_")).OrderBy(r => r.cmpRes).FirstOrDefault();
            if (topMatch != null)
            {
                var clk = clicks.FirstOrDefault(r => r.Desc == topMatch.extraInfo);
                if (clk == null)
                {
                    context.InfoLog("!!!!!!!!!!!!!!!!!! Error: Failed to get match data " + sb.ToString() + " " + topMatch);
                }
                context.MoveMouseAndClick(topMatch.x + clk.point.x, topMatch.y + clk.point.y);
                context.InfoLog("StandClick topMatch " + topMatch);                                                
                context.LogMatchAnalyst($"-name {clk.Desc} -match {clk.ImageName} {clk.Threadshold} ", topMatch.cmpRes);
                
            }
            return results;
        }
        public class ImgChecksAndTags
        {
            public string ImageName { get; private set;}
            public string Desc { get; private set; }
            public ccPoint point;
            public decimal Threadshold;
            public ImgChecksAndTags(string imgName, string desc, ccPoint pt = null, decimal threadshold = 200000)
            {
                ImageName = "data\\check\\"+imgName;
                Desc = desc;
                point = pt;
                Threadshold = threadshold;
                if (pt == null)
                {
                    var img = new Bitmap(ImageName);
                    point = new ccPoint(img.Width / 2, img.Height / 2);
                }
            }
        }
        static List<ImgChecksAndTags> clicks = new List<ImgChecksAndTags>
        {
            new ImgChecksAndTags("ememyattacked.png", "PRMXYCLICK_STD_EnemyAttacked", new ccPoint(345, 440), 2000),
            new ImgChecksAndTags("ccNotResponding.png", "PRMXYCLICK_STD_ccNotResponding", new ccPoint(375, 101)),
            new ImgChecksAndTags("loadVillage.png", "PRMXYCLICK_STD_LoadingVillage", new ccPoint(298, 44)),
            new ImgChecksAndTags("accountlist.png", "PRMXYCLICK_STD_AccountList", new ccPoint(101, 85)),
            new ImgChecksAndTags("confirmLoadAreYouSure.png", "PRMXYCLICK_STD_ConfirmLoadVillage", new ccPoint(402, 22)),
            new ImgChecksAndTags("confirmready.png", "PRMXYCLICK_STD_ConfirmLoadVillageReady", new ccPoint(310, 22)),
            new ImgChecksAndTags("justbootup.png", "PRMXYCLICK_STD_CheckJustBootedUp", new ccPoint(52, 70)),
            new ImgChecksAndTags("clashofclanicon.png", "PRMXYCLICK_STD_StartGame", new ccPoint(44, 44)),
            new ImgChecksAndTags("leftexpand.png", "PRMXYCLICK_ACT_LeftExpand", null, 10), //new ccPoint(20, 66)
            new ImgChecksAndTags("chacha.png", "PRMXYCLICK_STD_Close"), //Point(12, 14)
            new ImgChecksAndTags("chacha_closeTrain.png", "PRMXYCLICK_STD_Close"),//, Point(22, 22)
            new ImgChecksAndTags("upgradeFailedClose.png", "PRMXYCLICK_STD_Close"),//, Point(22, 22)
            new ImgChecksAndTags("closematch.png", "PRMXYCLICK_STD_Close"),//, Point(22, 22)
            new ImgChecksAndTags("leftshrink.png", "PRMXYCLICK_STD_LeftShrink", null, 10),//Point(22, 64)
            new ImgChecksAndTags("chacha_settings.png", "PRMXYCLICK_STD_CloseSettings", null, 20),//Point(22, 22)
            new ImgChecksAndTags("anyoneThere.png", "PRMXYCLICK_STD_AnyoneThere", new ccPoint(63, 119), 5000),
            new ImgChecksAndTags("connectionError.png", "PRMXYCLICK_STD_ConnectionError", new ccPoint(53, 121), 400),
            new ImgChecksAndTags("connectionTryAgain.png","PRMXYCLICK_STD_ConnectionError", new ccPoint(46, 16), 400),
            new ImgChecksAndTags("returnHomeAfterFight.png", "PRMXYCLICK_STD_ReturnHome", null, 4000), //Point(50, 50)
            new ImgChecksAndTags("returnHomeAfterVisit.png", "PRMXYCLICK_STD_ReturnHomeVisit", null, 6000), //Point(50, 50)
        };
    }
}
