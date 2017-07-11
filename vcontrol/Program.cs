using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualBox;

namespace ConsoleApplication1
{
    class Program
    {

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
        static void Main(string[] args)
        {
            Console.WriteLine(runApp());
            return;
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
                    Console.WriteLine("multi support" + mouse.MultiTouchSupported);
                    for (int i = 0; i < 5; i++)
                    {
                        mouse.PutMouseEvent(-200, -200, 0, 0, 0);
                        Thread.Sleep(100);
                    }
                    
                    mouse.PutMouseEvent(200, 200, 0, 0, 0);
                    Thread.Sleep(100);
                    mouse.PutMouseEvent(200, 200, 0, 0, 0);
                    Thread.Sleep(100);
                    mouse.PutMouseEvent(115, 200, 0, 0, 0);
                    Thread.Sleep(100);
                    mouse.PutMouseEvent(0, 60, 0, 0, 1);
                    for (int i = 0; i < 5; i++)
                    {
                        mouse.PutMouseEvent(-200, -200, 0, 0, 0);
                        Thread.Sleep(100);
                    }
                    Thread.Sleep(100);
                    mouse.PutMouseEvent(200, 150, 0, 0, 0);
                    Thread.Sleep(100);
                    mouse.PutMouseEvent(160, 0, 0, 0, 1);
                    mouse.PutMouseEvent(160, 0, 0, 0, 0);

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
