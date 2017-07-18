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
        

        static void checkLoop(IMouse mouse, IKeyboard keyboard)
        {
            ProcessingContext context = new ProcessingContext
            {
                mouse = mouse
            };
            while (true)
            {                
                foreach (var cmd in Utils.runApp().Split('\n'))
                {
                    Console.Write(".");
                    bool stdClick = cmd.StartsWith("STDCLICK_");
                    //bool attacked = cmd.StartsWith("VillageAttacked");
                    //bool loadv = cmd.StartsWith("LoadingVillage");
                    bool confirm = cmd.StartsWith("STDCLICK_ConfirmLoadVillage");
                    //bool confirmReady = cmd.StartsWith("ConfirmLoadVillageReady");
                    //bool CheckJustBootedUp = cmd.StartsWith("CheckJustBootedUp");
                    //bool startGame = cmd.StartsWith("StartGame");
                    if (cmd.Contains("CheckJustBootedUp"))
                    {
                        keyboard.ReleaseKeys();
                        Thread.Sleep(100);
                        keyboard.PutScancode(0x2A);
                        Thread.Sleep(100);
                    }
                    if (stdClick)
                    {
                        Console.WriteLine(cmd);
                        var cmds = cmd.Split(' ');
                        var x = Convert.ToInt32(cmds[1]);
                        var y = Convert.ToInt32(cmds[2]);
                        var cmpRes = Convert.ToDecimal(cmds[3]);
                        var command = cmds[0];
                        if (ProcessCommand(context, new CommandInfo { command = command, cmpRes = cmpRes, x = x, y= y })) continue;
                        if (command == "STDCLICK_LeftExpand") continue;
                        Utils.MouseMouseTo(mouse, x, y);
                        Utils.MouseClick(mouse);
                        if (confirm)
                        {
                            //MouseMouseTo(mouse, x - 200, y);
                            //MouseClick(mouse);
                            //                            keyboard.PutScancodes(codes);
                            mouse.PutMouseEvent(-200, 0, 0, 0, 0);
                            Utils.MouseClick(mouse);
                            Utils.SendString(keyboard, "CONFIRM");

                            Thread.Sleep(1000);
                            mouse.PutMouseEvent(200, 0, 0, 0, 0);
                            Utils.MouseClick(mouse);
                            Thread.Sleep(2000);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        private static bool ProcessCommand(ProcessingContext context, CommandInfo cmd)
        {
            switch (cmd.command)
            {
                case "STDCLICK_LeftExpand":
                    //TODO: add back
                    ProcessDonate(context, cmd);
                    break;
                case "STDCLICK_TrainTroops":
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

        

        static void Main(string[] args)
        {                       
            var vbox = new VirtualBox.VirtualBox();
            VirtualBox.IMachine machine = vbox.FindMachine("cctest");
            VirtualBoxClient vboxclient = new VirtualBoxClient();
            var session = vboxclient.Session;            
            try
            {
                machine.LockMachine(session, LockType.LockType_Shared);
                var console = session.Console;
                IEventSource es = console.EventSource;
                var listener = es.CreateListener();
                Array listenTYpes = new VBoxEventType[] { VBoxEventType.VBoxEventType_InputEvent };
                es.RegisterListener(listener, listenTYpes, 0);
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
                Console.WriteLine(e.Message);

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
