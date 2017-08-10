using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ccVcontrol
{
    public class PcWakeup
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateWaitableTimer(IntPtr lpTimerAttributes,
        bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll")]
        static extern bool SetWaitableTimer(IntPtr hTimer, [In] ref long
        pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr
        lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
         static extern Int32 WaitForSingleObject(IntPtr handle, uint
        milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);


        public static void SetWaitForWakeUpTime(IVDController controller, long timeMs, bool setToSleep)
        {
            
            IntPtr handle;
            //long duetime = -300000000; // negative value, so a RELATIVE due time
            long duetime = -10000*timeMs; // negative value, so a RELATIVE due time
            handle = CreateWaitableTimer(IntPtr.Zero, true, "MyWaitabletimer");
            SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero, IntPtr.Zero, true);
            if (setToSleep)
            {
                new Thread(() =>
                {
                    controller.Log("info", "puting computer to sleep");
                    PutComputerToSleep();
                }).Start();
            }
            uint INFINITE = 0xFFFFFFFF;
            int ret = WaitForSingleObject(handle, INFINITE);
            CloseHandle(handle);
            controller.Log("info", "computer woke from sleep");
        }

        public static void PutComputerToSleep()
        {
            SetSuspendState(false, false, false);
        }
    }
}
