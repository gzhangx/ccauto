using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualBox;

namespace ConsoleApplication1
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        static string runApp()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = "-check";
            start.FileName = "D:\\gang\\rctest\\x64\\test\\temptest.exe";
            start.WorkingDirectory = "D:\\gang\\rctest";
            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.UseShellExecute = false;
            int exitCode;


            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start))
            {
                var result = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();                
                // Retrieve the app's exit code
                exitCode = proc.ExitCode;
                return result;
            }           
        }

        static void MouseMouseTo(IMouse mouse, int x, int y)
        {
            for (int i = 0; i < 5; i++)
            {
                mouse.PutMouseEvent(-200, -200, 0, 0, 0);
                Thread.Sleep(100);
            }

            const int MAX = 200;
            while(x > MAX || y > MAX)
            {
                int mx = x > MAX ? MAX : x;
                x -= mx;
                int my = y > MAX ? MAX : y;
                y -= my;
                mouse.PutMouseEvent(mx, my, 0, 0, 0);
                Thread.Sleep(100);
            }
            mouse.PutMouseEvent(x, y, 0, 0, 0);
        }
        static void MouseClick(IMouse mouse)
        {
            Thread.Sleep(100);
            mouse.PutMouseEvent(0, 0, 0, 0, 1);
            Thread.Sleep(100);
            mouse.PutMouseEvent(0, 0, 0, 0, 0);
        }

        static uint GetScanCode(char c)
        {
            //MAPVK_VK_TO_VSC = 0x00;            
            short cd = VkKeyScanEx(c, IntPtr.Zero);
            return MapVirtualKeyEx(((uint)cd&0xff),0, IntPtr.Zero);
        }
        static void checkLoop(IMouse mouse, IKeyboard keyboard)
        {
            while (true)
            {
                foreach (var cmd in runApp().Split('\n'))
                {
                    bool attacked = cmd.StartsWith("VillageAttacked");
                    bool loadv = cmd.StartsWith("LoadingVillage");
                    bool confirm = cmd.StartsWith("ConfirmLoadVillage");                    
                    if (attacked || loadv || confirm)
                    {
                        Console.WriteLine(cmd);
                        var cmds = cmd.Split(' ');
                        var x = Convert.ToInt32(cmds[1]);
                        var y = Convert.ToInt32(cmds[2]);                        
                        MouseMouseTo(mouse, x, y);
                        MouseClick(mouse);
                        if (confirm)
                        {
                            //MouseMouseTo(mouse, x - 200, y);
                            //MouseClick(mouse);
                            //                            keyboard.PutScancodes(codes);
                            mouse.PutMouseEvent(-200, 0, 0, 0, 0);
                            MouseClick(mouse);

                            SendString(keyboard, "CONFIRM");
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private static void SendString(IKeyboard keyboard, string str)
        {
            var codes = new short[str.Length];
            Console.WriteLine("writiting");
            Thread.Sleep(1000);
            //keyboard.PutScancode(0x2A); //shift
            for (int i = 0; i < str.Length; i++)
            {
                codes[i] = (short)GetScanCode(str[i]);
                Console.WriteLine("code for " + str[i] + " is " + codes[i].ToString("X"));
                keyboard.PutScancode((int)codes[i]);
                Thread.Sleep(200);
                keyboard.PutScancode((int)codes[i] | 0x80);
                Thread.Sleep(300);
            }
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
                    MouseMouseTo(mouse, 515, 660);
                    MouseClick(mouse);
                    MouseMouseTo(mouse, 360, 156);
                    MouseClick(mouse);
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
