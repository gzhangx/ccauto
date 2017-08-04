using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class SimpleController : IVDController
    {
        public int[] accountStartCounts;
        public SimpleController()
        {
            string tmp;
            var files = SwitchAccount.GetAccountFiles(out tmp);
            accountStartCounts = new int[files.Count()];
        }
        public bool canContinue()
        {
            bool keepGoing = false;
            for (var i = 0; i < accountStartCounts.Length;i++)
            {
                Console.WriteLine($"for account {i} got {accountStartCounts[i]}");
                if (accountStartCounts[i] == 0)
                {
                    keepGoing = true;
                }
            }
            return keepGoing;
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
            accountStartCounts[act - 1]++;
            Console.WriteLine($"=======================> Starting account {act}");
        }
    }
}
