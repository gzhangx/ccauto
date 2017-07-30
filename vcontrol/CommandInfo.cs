using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class CommandInfo
    {
        public string command;
        public int x;
        public int y;
        public decimal cmpRes;
        public string Text;
        public string decision;
        public string extraInfo;
        public override string ToString()
        {
            return $"CommandInfo: {command} ({x},{y}):{cmpRes} {Text} d={decision} {extraInfo}";
        }
    }
}
