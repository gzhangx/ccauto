﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class SimpleController : IVDController
    {
        public int[] accountStartCounts;
        protected ILog Logger;
        public Action<ProcessingContext> CustomAct;
        public SimpleController()
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.conf"));
            Logger = LogManager.GetLogger("ccVcontrol");
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

        public virtual void CustomAction(ProcessingContext context)
        {
            if (CustomAct != null) CustomAct(context);
        }

        public void Log(string type, string msg)
        {
            switch (type)
            {
                case "debug":
                    //Console.WriteLine(" " + msg);
                    Logger.Debug(msg);
                    break;
                case "info":
                    //Console.WriteLine("=>" + msg);
                    Logger.Info(msg);
                    break;
                default:
                    //Console.WriteLine("." + msg);
                    Logger.Info("." + msg);
                    break;
            }
        }

        public void LogMatchAnalyst(string str, decimal res)
        {
            Log("debug",$"...LogMatchAnalyst {str}/{res}");
            //format  -input tstimgs\chk_act_6cnf.png -match data\check\confirm.bmp 10000/8038.000000
            int ind = str.IndexOf("-match");
            if (ind > 0)
            {
                try
                {
                    var nnn = str.Substring(ind);
                    var parts = nnn.Split(' ');
                    var fname = parts[1];
                    var matchres = parts[2];
                    //InfoLog($"  LogMatchAnalyst==>{fname} {res.ToString("0")}/{matchres}");
                    Log("matchAnalyst", $"{fname} {res.ToString("0")}/{matchres}");
                }
                catch (Exception exc)
                {
                    Log("error",$"Bad format exc for LogMatchAnalyst {str} {exc}");
                }
            }
            else
            {
                Log("error","Bad format for LogMatchAnalyst " + str);
            }
        }

        public void NotifyStartingAccount(IAccountControl act)
        {
            accountStartCounts[act.CurAccount - 1]++;
            Logger.Info($"=======================> Starting account {act.CurAccount}");
        }

        public void Sleep(int ms)
        {
            //Logger.Debug($"               Sleeping {ms}");
            Thread.Sleep(ms);
        }
    }
}
