using ccVcontrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vcConsole
{
    class Program
    {
        static void Main(string[] args)
        {            
            while (true)
            {
                ccVcontrol.Program.Start(new SimpleController());
                Console.WriteLine("Sleeping");
                Thread.Sleep(1000 * 60 * 10);
                Console.WriteLine("Done Sleeping");
            }
        }
    }

}
