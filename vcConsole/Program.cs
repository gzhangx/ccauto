using ccVcontrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ccVcontrol.Program.Start(new SimpleController());
        }
    }

}
