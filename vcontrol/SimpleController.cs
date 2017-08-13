using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class SimpleController : IVDController
    {
        public bool doUpgrades { get; set; }
        public bool doDonate { get; set; }
        protected int[] accountStartCounts;
        protected ILog Logger;
        public Action<ProcessingContext> CustomAct;
        public Action<string, string> EventNotify;
        public SimpleController()
        {
            doUpgrades = true;
            doDonate = true;
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
            EventNotify?.Invoke(type, msg);
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
            if (act.CurAccount < 1) return;
            accountStartCounts[act.CurAccount - 1]++;
            EventNotify?.Invoke("startingAccount", act.CurAccount.ToString());
            Logger.Info($"=======================> Starting account {act.CurAccount}");
        }

        protected object syncObj = new object();
        protected string doInterrupt = null;
        public void Sleep(int ms, bool deep = false)
        {
            if (deep)
            {
                PcWakeup.SetWaitForWakeUpTime(this, ms, true);
                return;
            }
            //Logger.Debug($"               Sleeping {ms}");
            lock (syncObj)
            {
                Monitor.Wait(syncObj, ms);
                if (doInterrupt != null)
                {
                    doInterrupt = null;
                    throw new SwitchProcessingActionException(doInterrupt);
                }
            }
        }

        public ProcessState CurState { get; set; }

        protected void InterruptProcessing(string reason)
        {
            lock(syncObj)
            {
                doInterrupt = reason;
                Monitor.PulseAll(syncObj);
            }
        }

        protected int switchingToAccount = -1;
        public void ChangeToNewAccount(int act)
        {
            CurState = ProcessState.SwitchAccount;
            switchingToAccount = act;
            InterruptProcessing("Chaning account");            
        }

        public void Init()
        {
            accountStartCounts = new int[accountStartCounts.Length];
        }

        public bool DoDonate()
        {
            return CurState == ProcessState.Normal;
        }
        public bool DoBuilds()
        {
            return CurState == ProcessState.Normal;
        }
        public void DoneCurProcessing()
        {
            CurState = ProcessState.Normal;
            switchingToAccount = -1;
        }
        public int CheckSetCurAccount(int act)
        {
            if (CurState == ProcessState.SwitchAccount)
            {
                return switchingToAccount;
            }
            return act;
        }


        public void RefreshNetwork()
        {
            string res = Utils.runAnyApp("netsh", "interface set interface \"Ethernet 2\" admin=disable");
            Log("info", "Disable network " + res);
            res = Utils.runAnyApp("netsh", "interface set interface \"Ethernet 2\" admin=enable");
            Log("info", "Enable network " + res);
            Sleep(4000);
        }

        public void KillVBox()
        {
            var procs = Process.GetProcessesByName("VirtualBox");
            foreach (var proc in procs)
            {
                Log("important", "Killing vbox " + proc.ProcessName);
                try
                {
                    proc.Kill();
                } catch (Exception exc)
                {
                    Log("important", "Killing vbox failed " + proc.ProcessName);
                }
            }
        }
    }
}
