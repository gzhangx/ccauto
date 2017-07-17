using System;
using System.Collections.Generic;
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

        public static void MouseMouseTo(IMouse mouse, int x, int y)
        {
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
        public static void MouseClick(IMouse mouse)
        {
            Thread.Sleep(100);
            mouse.PutMouseEvent(0, 0, 0, 0, 1);
            Thread.Sleep(100);
            mouse.PutMouseEvent(0, 0, 0, 0, 0);
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
    }
}
