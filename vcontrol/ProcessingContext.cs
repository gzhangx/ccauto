﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualBox;

namespace ccVcontrol
{
    public class ProcessingContext
    {
        public IMouse mouse;

        public int DoStdClicks(ProcessingContext context, List<CommandInfo> clicks)
        {
            int count = 0;
            foreach (var cmd in clicks)
            {
                if (cmd.command.StartsWith("STDCLICK_"))
                {
                    count++;
                    Utils.MoveMouseAndClick(mouse, cmd.x, cmd.y);
                }
            }
            return count;
        }

        public void MouseMouseTo(int x, int y)
        {
            Utils.MouseMouseTo(mouse, x, y);
        }

        public void MouseMouseRelative(int x, int y)
        {
            Utils.MouseMouseRelative(mouse, x, y);
        }
        public void MouseClick()
        {
            Utils.MouseClick(mouse);
        }

        public void MoveMouseAndClick(IMouse mouse, int x, int y)
        {
            Utils.MoveMouseAndClick(mouse, x, y);
        }

    }
}
