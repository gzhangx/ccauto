using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualBox;

namespace ccVcontrol
{
    public interface IVDController
    {
        bool humanMode { get; }
        bool dontSleepOrShutdown { get; set; }
        bool switchAccountOnly { get; }
        bool doUpgrades { get; }
        bool doDonate { get; }
        bool redoStructureNames { get; }
        bool canContinue();
        void NotifyStartingAccount(IAccountControl act);
        void Log(string type, string msg);
        void LogMatchAnalyst(string str, decimal res);
        void Sleep(int ms, bool deep = false);
        void CustomAction(ProcessingContext context);
        void Init();

        ProcessState CurState { get; }
        void DoneCurProcessing();
        bool DoDonate();
        bool DoBuilds();
        int CheckSetCurAccount(int act);
        void KillVBox();
    }

    public enum ProcessState
    {
        Normal,
        SwitchAccount,
    }

    public class SwitchProcessingActionException: Exception
    {
        public SwitchProcessingActionException(string msg) : base(msg) { }
    }

    public interface IAccountControl
    {
        int CurAccount { get; set; }
        //void SwitchAccount(int act);
    }
    public class ProcessingContext
    {
        public IMouse mouse;
        public IKeyboard keyboard;
        public IVDController vdcontroller;

        private StandardClicks stdClk;
        public ProcessingContext(IMouse m, IKeyboard k, IVDController c)
        {
            mouse = m;
            keyboard = k;
            vdcontroller = c;
            stdClk = new StandardClicks(this);
        }
        public void Sleep(int ms)
        {
            vdcontroller.Sleep(ms);
        }
        public virtual void SendString(string str)
        {
            Utils.SendString(keyboard, str, this);
        }

        public void DebugLog(string str)
        {
            vdcontroller.Log("debug", str);
            //Console.WriteLine(" " + str);
        }
        public void InfoLog(string str)
        {
            vdcontroller.Log("info", str);
            //Console.WriteLine("=>" + str);
        }

        public void LogMatchAnalyst(string str, decimal res)
        {
            vdcontroller.LogMatchAnalyst(str, res);
        }

        internal List<CommandInfo> GetAppInfo(string argument = "-check")
        {
            return Utils.GetAppInfo(argument, this);
        }

        public List<CommandInfo> DoStdClicks()
        {
            MouseMouseTo(0, 0);
            var clicks = stdClk.Processing();
            return clicks;
        }

        public virtual void MouseMouseTo(int x, int y)
        {
            Utils.MouseMouseTo(mouse, x, y, this);
        }

        public virtual void MouseMouseRelative(int x, int y)
        {
            Utils.MouseMouseRelative(mouse, x, y);
        }
        public virtual void MouseClick()
        {
            Utils.MouseClick(mouse, this);
        }

        public virtual void MoveMouseAndClick(int x, int y)
        {
            Utils.MoveMouseAndClick(mouse, x, y, this);
        }

        public void DoShift()
        {
            keyboard.ReleaseKeys();
            Sleep(1000);
            keyboard.PutScancode(0x2A);
            Sleep(200);
        }


        protected bool CheckFound(List<CommandInfo> cmds, string toFind)
        {
            var cmd = cmds.FirstOrDefault(c => c.extraInfo == "PRMXYCLICK_ACT_LeftExpand");
            if (cmd == null) return false;
            return cmd.decision == "true";
        }
        public List<CommandInfo> GetToEntrance()
        {
            DateTime start = DateTime.Now;
            while (true)
            {                
                DebugLog(" CheckEntrance");
                var cmds = DoStdClicks();
                DebugLog(" CheckEntrance.getAppInfo");
                if (CheckFound(cmds, "PRMXYCLICK_ACT_LeftExpand"))
                {
                    DebugLog(" CheckEntrance.FoundLoaded");                    
                    return cmds;
                }
                Sleep(5000);
                var elapsed = DateTime.Now.Subtract(start).TotalMinutes;
                if (elapsed > 5)
                {
                    var errStr = "Waiting for entracne too long " + elapsed.ToString("0.00");
                    vdcontroller.Log("error", errStr);
                    if (!vdcontroller.humanMode)
                        throw new TimeoutException(errStr);
                }
            }
        }
    }
}
