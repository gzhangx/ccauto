using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualBox;

namespace ccVcontrol
{
    
    
    public class Program
    {        
        static void checkLoop(ProcessingContext context)
        {
            //Utils.doScreenShoot("tstimgs\\full.png");
            
            context.DebugLog("Getting app info");            
            //cmds = Utils.GetAppInfo("-name allfull -screenshoot");
            //cmds = Utils.GetAppInfo("-name c5 -matchRect 79,32,167,22_200 -screenshoot");            
            context.GetToEntrance();
            context.DebugLog("Do shift");
            context.DoShift();
            var controller = context.vdcontroller;
            controller.Init();
            while (controller.canContinue())
            {
                try
                {
                    controller.CustomAction(context);
                    DoProcess(context);
                } catch (SwitchProcessingActionException swa)
                {
                    controller.Log("info", "switch action " + swa.Message);
                }
            }
        }

        private static void DoProcess(ProcessingContext context)
        {
            var controller = context.vdcontroller;
            var switchAccount = new SwitchAccount(context);
            context.GetToEntrance();
            context.Sleep(2000);
            int acct = switchAccount.CheckAccount();
            if (acct <= 0)
            {
                context.InfoLog("failed to get account, try again");
                context.Sleep(4000);
                acct = switchAccount.CheckAccount();
            }
            context.InfoLog($"===>Step gen acct pic {acct}");
            switchAccount.CurAccount = acct;
            GenerateAccountPics(context, switchAccount.CurAccount);            
            context.InfoLog($"===>Step Using account {switchAccount.CurAccount}");
            context.vdcontroller.NotifyStartingAccount(switchAccount);            
            if (controller.DoDonate() && controller.doDonate)
            {
                context.InfoLog("===>Step Donate");
                //cmds = Utils.GetAppInfo();                
                ProcessDonate(context, context.GetToEntrance());
                context.GetToEntrance();
            }
            if (controller.DoBuilds())
            {
                context.InfoLog("===>Step textmap");
                new ProcessorMapByText(context).ProcessCommand(acct);
            }
            switchAccount.CurAccount = controller.CheckSetCurAccount(acct);
            context.InfoLog("===>Step SwitchAccount");
            switchAccount.Process();
            context.InfoLog("===>Step get to entrance");
            context.GetToEntrance();
            context.Sleep(4000);
            controller.DoneCurProcessing();
        }

        private static void GenerateAccountPics(ProcessingContext context, int who)
        {
            context.DebugLog("------------------Generate account pics");
            context.GetToEntrance();
            var fullImg = $"tstimgs\\accountFull_{who}.png";
            Utils.doScreenShoot(fullImg);
            //Utils.GetAppInfo($"-name data\\accounts\\img_act{who}.png -input {fullImg} {SwitchAccount.acctNameMatchRect} -imagecorp");
        }

    
        private static void ProcessDonate(ProcessingContext context, List<CommandInfo> cmds)
        {
            var cmd = cmds.FirstOrDefault(c => c.extraInfo == "PRMXYCLICK_ACT_LeftExpand");
            if (cmd != null)
                new ProcessorDonation(context).ProcessDonate(cmd);
        }


        static void TestAccounts(ProcessingContext context)
        {
            Utils.doScreenShoot("tstimgs\\tmptesttest.png");
            var actr = ProcessorMapByText.canUpgrade(context, "tstimgs\\tmptesttest.png");
            Console.WriteLine(actr.upgrade);
            /*
            for (int i = 1; i <= 4; i++)
            {
                //generate mask
                //Utils.GetAppInfo($"-name data\\accounts\\img_act{i}.png -input tstimgs\\full_act_full_{i}.png -matchRect 80,30,100,27_200 -imagecorp");
            }
            for (int i = 1; i <= 4; i++)
            {
                //generate mask
                //Utils.GetAppInfo($"-name data\\accounts\\img_act{i}.png -input tstimgs\\full_act_full_{i}.png -matchRect 80,30,100,27_200 -imagecorp");
                var res = context.GetAppInfo($"-name cmpact{i} -input tstimgs\\accountFull_1.png {SwitchAccount.acctNameMatchRect} -match data\\accounts\\img_act{i}.png 10000");
                foreach (var r in res)
                {
                    Console.WriteLine(r);
                }
            }
            */
        }
        public static void Start(IVDController controller)
        {
            //var rrr = Utils.GetAppInfo($"-input tstimgs\\upgradeelibad.png -name upgradegood -match data\\check\\upgradeeligood.png 10000 -name upgradebad -match data\\check\\upgradeelibad.png 10000");
            //SwitchAccount.CheckAccount();
            //TestAccounts(); //TODO DEBUG REMOVE THIS 
            //return;
            controller.Log("info","Starting vm");
            Utils.executeVBoxMngr($"startvm {Utils.vmname}");
            controller.Log("info", "VmStarted, allocate machine");
            var vbox = new VirtualBox.VirtualBox();
            VirtualBox.IMachine machine = vbox.FindMachine(Utils.vmname);
            VirtualBoxClient vboxclient = new VirtualBoxClient();
            var session = vboxclient.Session;            
            try
            {
                controller.Log("info", "found machine, lock machine");
                machine.LockMachine(session, LockType.LockType_Shared);
                var console = session.Console;
                IEventSource es = console.EventSource;
                var listener = es.CreateListener();
                Array listenTYpes = new VBoxEventType[] { VBoxEventType.VBoxEventType_InputEvent };
                es.RegisterListener(listener, listenTYpes, 0);
                controller.Log("info", "locked machine, entry try");
                try
                {
                    //session.Console.Display.SetSeamlessMode(1);
                    var display = console.Display;
                    uint sw, sh, bpp;
                    int xorig, yorig;
                    GuestMonitorStatus gmstat;
                    display.GetScreenResolution(0, out sw, out sh, out bpp, out xorig, out yorig, out gmstat);
                    //Console.WriteLine($"sw={sw} {sh} bpp {bpp} xorig={xorig} yorig={yorig}");

                    byte[] buf = new byte[sw * sh * bpp / 8];
                    //display.TakeScreenShot(0, ref buf[0], sw, sh, BitmapFormat.BitmapFormat_PNG);

                    var mouse = session.Console.Mouse;
                    var keyboard = session.Console.Keyboard;
                    //MouseMouseTo(mouse, 515, 660);
                    //MouseClick(mouse);
                    //MouseMouseTo(mouse, 360, 156);
                    //MouseClick(mouse);
                    controller.Log("info","main loop");
                    while (true)
                    {
                        try
                        {
                            checkLoop(new ProcessingContext(mouse, keyboard, controller));
                        } catch (SwitchProcessingActionException)
                        {
                            continue;
                        }
                        break;
                    }
                }
                finally
                {
                    es.UnregisterListener(listener);
                }
            }
            catch (Exception e)
            {
                controller.Log("error", e.ToString());

            }
            finally
            {                
                if (session.State == SessionState.SessionState_Locked)
                    session.UnlockMachine();
                controller.Log("info", "powering off");
                Utils.executeVBoxMngr($"controlvm {Utils.vmname} poweroff");
                controller.KillVBox();
            }
            //Console.WriteLine(machine.VideoCaptureWidth);            
        }
    }
}
