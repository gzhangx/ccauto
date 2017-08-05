using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class SwitchAccount : BaseProcessor
    {
        public const string acctNameRect = "100,27";
        public const string acctNameCorpRect = "101,28";
        public static string acctNameMatchRect = $"-matchRect 80,10,{acctNameCorpRect}_200";
        static string acctNameCorpMatchRect = $"-matchRect 0,0,{acctNameCorpRect}_200";
        const int ACCOUNTSPACING = 72;
        const string accoungImagDir = "data\\accounts\\";
        protected int account = 0;
        public int CurAccount
        {
            get { return account; }
            set { account = value; }
        }
        static int MAXACCOUNT = 6;
        public SwitchAccount(ProcessingContext ctx) : base(ctx)
        {
            switchSteps = new List<StepInfo>
            {
                new StepInfo { inputName= "chk_act_1stb.png", cmd = "-match settingsbutton.png 10", maxRetry = 2, name = "FindSettingsButton", xoff = 10, yoff = 10, delay = 1000 },
                new StepInfo { inputName= "chk_act_2psi.png", cmd = "-match googlePlaySignIn.png 4000", maxRetry = 3, name = "FindPlaySignin", xoff = 101, yoff = 63, delay = 5000, otherStepCheck = new [] { "FindPlaySigninDisconnected" } },
                new StepInfo { inputName= "chk_act_3dsc.png", cmd = "-match googlePlayDisconnected.png 4000", maxRetry = 3, name = "FindPlaySigninDisconnected", xoff = 101, yoff = 63 , delay = 3000},
                new StepInfo { inputName= "chk_act_4als.png", cmd = "-match accountlist.png 6000", maxRetry = 5, name = "SwitchAccount", xoff = 107, yoff = 89, Act = SwitchAccountAction , delay = 15000},


                new StepInfo { inputName= "chk_act_5ldv.png", cmd = "-match loadVillage.png 1000", maxRetry = 5, name = "LoadVillage", xoff = 298, yoff = 44, delay=5000 },
                new StepInfo { inputName= "chk_act_6cnf.png", cmd = "-match confirm.bmp 10000", maxRetry = 5, name = "ConfirmLoadVillage", xoff = 310, yoff = 22, Act= ConfirmLoadVillage },          
            };
        }

        public static int CheckAccount()
        {
            const string tempName = "tstimgs\\tempFullScreen.png";            
            Utils.doScreenShoot(tempName);
            const string tempPartName = "tstimgs\\tempName.png";
            Utils.GetAppInfo($"-name {tempPartName} -input {tempName} {acctNameMatchRect} -imagecorp");            
            return CheckAccountWithImage(tempPartName);
        }
        public static IEnumerable<string> GetAccountFiles(out string imgStart)
        {
            var actImgNameStart = accoungImagDir + "img_act";
            imgStart = actImgNameStart;
            var files = System.IO.Directory.GetFiles(accoungImagDir).Where(f => f.StartsWith(actImgNameStart));
            return files;
        }
        public static int CheckAccountWithImage(string screenName)
        {
            var actImgNameStart = accoungImagDir + "img_act";
            var files = GetAccountFiles(out actImgNameStart);
            MAXACCOUNT = files.Count();
            CommandInfo good = null;
            StringBuilder sb = new StringBuilder();
            sb.Append($" -input {screenName} ").Append(acctNameCorpMatchRect);
            foreach (var f in files)
            {
                var actname = f.Substring(actImgNameStart.Length);
                actname = actname.Substring(0, actname.IndexOf("."));
                sb.Append($" -name {actname} -match {f} 96876875");                
            }
            var res = Utils.GetAppInfo(sb.ToString());
            foreach (var r in res)
            {
                if (good == null) good = r;
                if (r.command == "SINGLEMATCH")
                {
                    if (r.decision == "true")
                    {
                        if (r.cmpRes < good.cmpRes) good = r;
                    }
                }
                Console.WriteLine(r);
            }
            if (good == null) return -1;
            return Convert.ToInt32(good.extraInfo);
        }

        public override StepContext Process()
        {
            context.InfoLog($"Trying to find SINGLEMATCH for settings button account ----- {account}");
            switchSteps.First(r => r.name == "SwitchAccount").Act = SwitchAccountAction;
            var res = DoSteps(switchSteps);            
            return res;
        }

        private void SwitchAccountAction(CommandInfo found, StepInfo cur, StepContext stepContext)
        {
            account++;
            if (account >= MAXACCOUNT || account < 0) account = 0;

            context.MoveMouseAndClick(found.x + cur.xoff, found.y + cur.yoff + (account* ACCOUNTSPACING));
            context.MouseMouseTo(0, 0);
            
        }

        private void ConfirmLoadVillage(CommandInfo found, StepInfo cur, StepContext stepContext)
        {
            context.MoveMouseAndClick(found.x + cur.xoff, found.y + cur.yoff);
            context.mouse.PutMouseEvent(-200, 0, 0, 0, 0);
            context.MouseClick();
            context.MouseMouseTo(0, 0);
            context.Sleep(1000);
            context.SendString("CONFIRM");
            context.Sleep(1000);
            context.MoveMouseAndClick(found.x + cur.xoff, found.y + cur.yoff);
        }
        List<StepInfo> switchSteps;

    }
}
