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
    
    
    class Program
    {
        const string acctNameMatchRect = SwitchAccount.acctNameMatchRect;

        static void checkLoop(IMouse mouse, IKeyboard keyboard)
        {
            ProcessingContext context = new ProcessingContext
            {
                mouse = mouse,
                keyboard = keyboard,
            };

            var switchAccount = new SwitchAccount(context);
            context.DebugLog("Getting app info");
            var cmds = Utils.GetAppInfo();
            //cmds = Utils.GetAppInfo("-name allfull -screenshoot");
            //cmds = Utils.GetAppInfo("-name c5 -matchRect 79,32,167,22_200 -screenshoot");
            context.DebugLog("Do shift");
            context.DoShift();            
            
            while (true)
            {
                //cmds = Utils.GetAppInfo();
                cmds = context.GetToEntrance();

                switchAccount.Process();

                context.DoStdClicks(cmds);
                cmds = Utils.GetAppInfo();

                Thread.Sleep(100);

                GenerateAccountPics(switchAccount.CurAccount);
                Console.WriteLine("press enter to countinue");
                Console.ReadLine();
                
            }
        }

        private static void GenerateAccountPics(int who)
        {
            var fullImg = $"tstimgs\\accountFull_{who}.png";
            Utils.doScreenShoot(fullImg);
            Utils.GetAppInfo($"-name data\\accounts\\img_act{who}.png -input {fullImg} {acctNameMatchRect} -imagecorp");
        }

        private static bool ProcessCommand(ProcessingContext context, CommandInfo cmd)
        {
            switch (cmd.command)
            {
                case "PRMXYCLICK_ACT_LeftExpand":
                    //TODO: add back
                    ProcessDonate(context, cmd);
                    //new ProcessorMapByText(context).ProcessCommand();
                    break;
                case "PRMXYCLICK_STD_TrainTroops":
                    new ProcessorTrain(context).ProcessCommand(cmd);
                    break;
                default: return false;
            }            
            return true;
        }


        private static void ProcessDonate(ProcessingContext context, CommandInfo cmd)
        {
            new ProcessorDonation(context).ProcessDonate(cmd);
        }


        static void TestAccounts()
        {
            for (int i = 1; i <= 4; i++)
            {
                //generate mask
                //Utils.GetAppInfo($"-name data\\accounts\\img_act{i}.png -input tstimgs\\full_act_full_{i}.png -matchRect 80,30,100,27_200 -imagecorp");
            }
            for (int i = 1; i <= 4; i++)
            {
                //generate mask
                //Utils.GetAppInfo($"-name data\\accounts\\img_act{i}.png -input tstimgs\\full_act_full_{i}.png -matchRect 80,30,100,27_200 -imagecorp");
                var res = Utils.GetAppInfo($"-name cmpact{i} -input tstimgs\\accountFull_1.png {acctNameMatchRect} -match data\\accounts\\img_act{i}.png 10000");
                foreach (var r in res)
                {
                    Console.WriteLine(r);
                }
            }
        }
        static void Main(string[] args)
        {
            SwitchAccount.CheckAccount();
            //TestAccounts();
            return;
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
                    checkLoop(mouse, keyboard);
                    

                    //mouse.PutEventMultiTouch(2,)
                    return;
                    Thread.Sleep(1000);
                    for (int i = 0; i < 5; i++)
                    {
                        mouse.PutMouseEvent(100, 100, 0, 0, 0);
                        Console.WriteLine("at " + i);
                        Thread.Sleep(1000);
                    }
                    /*
                    for (int i = 0; i < 10; i++)
                    {
                        mouse.PutMouseEvent(-i * 10-100, 0, 0, 0, 0);
                        Console.WriteLine("going to " + i);
                        Thread.Sleep(100);

                        var evt = es.GetEvent(listener, 1000);
                        if (evt != null)
                        {
                            Console.WriteLine(evt.GetType() + " " + evt.ToString());
                        }
                    }
                    */
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
