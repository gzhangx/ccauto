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
            var switchAccount = new SwitchAccount(context);
            context.DebugLog("Getting app info");            
            //cmds = Utils.GetAppInfo("-name allfull -screenshoot");
            //cmds = Utils.GetAppInfo("-name c5 -matchRect 79,32,167,22_200 -screenshoot");            
            context.GetToEntrance();
            context.DebugLog("Do shift");
            context.DoShift();            
            while (context.vdcontroller.canContinue())
            {                
                //cmds = Utils.GetAppInfo();
                context.GetToEntrance();
                DoDonate(context);
                context.GetToEntrance();
                int acct = SwitchAccount.CheckAccount();
                context.vdcontroller.NotifyStartingAccount(acct);
                new ProcessorMapByText(context).ProcessCommand(acct);
                switchAccount.Process();

                context.GetToEntrance();
                Thread.Sleep(100);
                
                GenerateAccountPics(context, switchAccount.CurAccount);

                //DoDonate(context, cmds);
                //Console.WriteLine("press enter to countinue");
                //Console.ReadLine();

            }
        }

        private static void GenerateAccountPics(ProcessingContext context, int who)
        {
            context.DebugLog("Generate account pics");
            context.GetToEntrance();
            var fullImg = $"tstimgs\\accountFull_{who}.png";
            Utils.doScreenShoot(fullImg);
            //Utils.GetAppInfo($"-name data\\accounts\\img_act{who}.png -input {fullImg} {SwitchAccount.acctNameMatchRect} -imagecorp");
        }

        private static  void DoDonate(ProcessingContext context)
        {
            var cmds = context.GetToEntrance();
            var cmd = cmds.FirstOrDefault(c => c.command == "PRMXYCLICK_ACT_LeftExpand");
            if (cmd != null)
            {
                ProcessDonate(context, cmd);
            }
        }
    
        private static void ProcessDonate(ProcessingContext context, CommandInfo cmd)
        {
            new ProcessorDonation(context).ProcessDonate(cmd);
        }


        static void TestAccounts()
        {
            Utils.doScreenShoot("tstimgs\\tmptesttest.png");
            var actr = ProcessorMapByText.canUpgrade("tstimgs\\tmptesttest.png");
            Console.WriteLine(actr.upgrade);
            return;
            for (int i = 1; i <= 4; i++)
            {
                //generate mask
                //Utils.GetAppInfo($"-name data\\accounts\\img_act{i}.png -input tstimgs\\full_act_full_{i}.png -matchRect 80,30,100,27_200 -imagecorp");
            }
            for (int i = 1; i <= 4; i++)
            {
                //generate mask
                //Utils.GetAppInfo($"-name data\\accounts\\img_act{i}.png -input tstimgs\\full_act_full_{i}.png -matchRect 80,30,100,27_200 -imagecorp");
                var res = Utils.GetAppInfo($"-name cmpact{i} -input tstimgs\\accountFull_1.png {SwitchAccount.acctNameMatchRect} -match data\\accounts\\img_act{i}.png 10000");
                foreach (var r in res)
                {
                    Console.WriteLine(r);
                }
            }
        }
        public static void Start(IVDController controller)
        {
            //var rrr = Utils.GetAppInfo($"-input tstimgs\\upgradeelibad.png -name upgradegood -match data\\check\\upgradeeligood.png 10000 -name upgradebad -match data\\check\\upgradeelibad.png 10000");
            //SwitchAccount.CheckAccount();
            TestAccounts(); //TODO DEBUG REMOVE THIS 
            //return;
            Console.WriteLine("Starting vm");
            Utils.executeVBoxMngr("startvm cctest");
            Console.WriteLine("VmStarted, allocate machine");
            var vbox = new VirtualBox.VirtualBox();
            VirtualBox.IMachine machine = vbox.FindMachine("cctest");
            VirtualBoxClient vboxclient = new VirtualBoxClient();
            var session = vboxclient.Session;            
            try
            {
                Console.WriteLine("found machine, lock machine");
                machine.LockMachine(session, LockType.LockType_Shared);
                var console = session.Console;
                IEventSource es = console.EventSource;
                var listener = es.CreateListener();
                Array listenTYpes = new VBoxEventType[] { VBoxEventType.VBoxEventType_InputEvent };
                es.RegisterListener(listener, listenTYpes, 0);
                Console.WriteLine("locked machine, entry try");
                try
                {
                    //session.Console.Display.SetSeamlessMode(1);
                    var display = console.Display;
                    uint sw, sh, bpp;
                    int xorig, yorig;
                    GuestMonitorStatus gmstat;
                    display.GetScreenResolution(0, out sw, out sh, out bpp, out xorig, out yorig, out gmstat);
                    Console.WriteLine($"sw={sw} {sh} bpp {bpp} xorig={xorig} yorig={yorig}");

                    byte[] buf = new byte[sw * sh * bpp / 8];
                    //display.TakeScreenShot(0, ref buf[0], sw, sh, BitmapFormat.BitmapFormat_PNG);

                    var mouse = session.Console.Mouse;
                    var keyboard = session.Console.Keyboard;
                    //MouseMouseTo(mouse, 515, 660);
                    //MouseClick(mouse);
                    //MouseMouseTo(mouse, 360, 156);
                    //MouseClick(mouse);
                    Console.WriteLine("main loop");
                    checkLoop(new ProcessingContext(mouse, keyboard, controller));                    
                }
                finally
                {
                    es.UnregisterListener(listener);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
            finally
            {                
                if (session.State == SessionState.SessionState_Locked)
                    session.UnlockMachine();
            }

            Console.WriteLine(machine.VideoCaptureWidth);
        }
    }
}
