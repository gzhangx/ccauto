using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccVcontrol
{
    class SimpleController : IVDController
    {
        public int[] accountStartCounts = new int[4];
        public bool canContinue()
        {
            return true;
        }

        public void Log(string type, string msg)
        {
            switch (type)
            {
                case "debug":
                    Console.WriteLine(" " + msg);
                    break;
                case "info":
                    Console.WriteLine("=>" + msg);
                    break;
                default:
                    Console.WriteLine("." + msg);
                    break;
            }
        }

        public void NotifyStartingAccount(int act)
        {
            Console.WriteLine($"=======================> Starting account {act}");
        }
    }
}
