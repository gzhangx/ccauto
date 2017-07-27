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
    public class Utils
    {
        [DllImport("user32.dll")]
        static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        //"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe" controlvm cctest screenshotpng test.png
        const string vboxman = "\"C:\\Program Files\\Oracle\\VirtualBox\\VBoxManage.exe\"";
        const int YOFF = 0;// 21;

        public static void MouseMouseTo(IMouse mouse, int x, int y)
        {
            y -= YOFF;
            for (int i = 0; i < 5; i++)
            {
                mouse.PutMouseEvent(-200, -200, 0, 0, 0);
                Thread.Sleep(100);
            }

            const int MAX = 200;
            while (x > MAX || y > MAX)
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

        public static void MouseMouseRelative(IMouse mouse, int x, int y)
        {

            mouse.PutMouseEvent(x, y, 0, 0, 0);
            Thread.Sleep(100);
        }
        public static void MouseClick(IMouse mouse)
        {
            Thread.Sleep(100);
            mouse.PutMouseEvent(0, 0, 0, 0, 1);
            Thread.Sleep(100);
            mouse.PutMouseEvent(0, 0, 0, 0, 0);
        }

        public static void MoveMouseAndClick(IMouse mouse, int x, int y)
        {
            MouseMouseTo(mouse, x, y);
            MouseClick(mouse);
        }

        public static uint GetScanCode(char c)
        {
            //MAPVK_VK_TO_VSC = 0x00;            
            short cd = VkKeyScanEx(c, IntPtr.Zero);
            return MapVirtualKeyEx(((uint)cd & 0xff), 0, IntPtr.Zero);
        }

        public static void SendString(IKeyboard keyboard, string str)
        {
            var codes = new short[str.Length];
            Console.Write("writiting ");
            Thread.Sleep(1000);
            //keyboard.PutScancode(0x2A); //shift
            for (int i = 0; i < str.Length; i++)
            {
                codes[i] = (short)GetScanCode(str[i]);
                Console.Write(str[i]);
                keyboard.PutScancode((int)codes[i]);
                Thread.Sleep(200);
                keyboard.PutScancode((int)codes[i] | 0x80);
                Thread.Sleep(300);
            }
            Console.WriteLine();
        }

        public static void doScreenShoot(string fname)
        {
            executeVBoxMngr("controlvm cctest screenshotpng " + fname);
        }
        public static string executeVBoxMngr(string arguments)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = arguments;
            start.FileName = vboxman;
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

        public static string runApp(string arguments = "-check")
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = arguments;
            start.FileName = "D:\\gang\\rctest\\x64\\Debug\\temptest.exe";
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

        public static List<CommandInfo> GetAppInfo(string arguments = "-check")
        {
            var res = new List<CommandInfo>();
            foreach (var cmd in Utils.runApp(arguments).Split(new char[]{ '\r','\n'}))
            {
                if (cmd.Length < 2) continue;
                if (cmd.StartsWith("ERR:"))
                {
                    Console.WriteLine($"{cmd.Trim()} arg={arguments}");
                    continue;
                }
                if (cmd.StartsWith("***** VIDEOINPUT LIBRARY")) continue;
                try
                {
                    if (cmd.StartsWith("RecoResult_"))
                    {
                        int sp = cmd.IndexOf(" ");
                        res.Add(new CommandInfo
                        {
                            command = cmd.Substring(0, sp),
                            Text = cmd.Substring(sp + 1).Trim()
                        });
                        continue;
                    }
                    var cmds = cmd.Split(' ');
                    var command = cmds[0];
                    int x = 0, y = 0;
                    decimal cmpRes = decimal.MaxValue;
                    if (cmds.Length > 2)
                    {
                        x = Convert.ToInt32(cmds[1]);
                        y = Convert.ToInt32(cmds[2]);
                    }
                    if (cmds.Length > 3)
                        cmpRes = Convert.ToDecimal(cmds[3]);
                    res.Add(new CommandInfo { command = command, cmpRes = cmpRes, x = x, y = y });
                } catch (Exception exc)
                {
                    Console.WriteLine("failed " + exc.Message + " " + cmd);
                    Console.WriteLine(exc);
                    throw exc;
                }
                
            }
            return res;
        }

        
    }
}
