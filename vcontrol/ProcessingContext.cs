using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualBox;

namespace ccVcontrol
{
    public class ProcessingContext
    {
        public IMouse mouse;
        public IKeyboard keyboard;
        public virtual void SendString(string str)
        {
            Utils.SendString(keyboard, str);
        }

        public void DebugLog(string str)
        {
            Console.WriteLine(" " + str);
        }
        public void InfoLog(string str)
        {
            Console.WriteLine("=>" + str);
        }
        public int DoStdClicks(List<CommandInfo> clicks)
        {
            int count = 0;
            foreach (var cmd in clicks)
            {
                if (cmd.command.StartsWith("PRMXYCLICK_STD_"))
                {
                    count++;
                    MoveMouseAndClick(cmd.x, cmd.y);
                }
            }
            return count;
        }

        public virtual void MouseMouseTo(int x, int y)
        {
            Utils.MouseMouseTo(mouse, x, y);
        }

        public virtual void MouseMouseRelative(int x, int y)
        {
            Utils.MouseMouseRelative(mouse, x, y);
        }
        public virtual void MouseClick()
        {
            Utils.MouseClick(mouse);
        }

        public virtual void MoveMouseAndClick(int x, int y)
        {
            Utils.MoveMouseAndClick(mouse, x, y);
        }

        public void DoShift()
        {
            keyboard.ReleaseKeys();
            Thread.Sleep(1000);
            keyboard.PutScancode(0x2A);
            Thread.Sleep(200);
        }


        public List<CommandInfo> GetToEntrance()
        {
            while (true)
            {
                var cmds = Utils.GetAppInfo();
                DebugLog("MainLoop CheckEntrance");
                DoStdClicks(cmds);
                DebugLog("MainLoop, CheckEntrance.getAppInfo");
                if (cmds.FirstOrDefault(c => c.command == "PRMXYCLICK_ACT_LeftExpand") != null)
                {
                    DebugLog("MainLoop, CheckEntrance.FoundLoaded");
                    return cmds;
                }
            }
        }
    }
}
