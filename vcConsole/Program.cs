using ccVcontrol;
using log4net;
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
            ILog Logger = LogManager.GetLogger("main");
            var controller = new SimpleController();
            while (true)
            {
                DateTime startTime = DateTime.UtcNow;
                Logger.Info("Starting");
                try
                {
                    ccVcontrol.Program.Start(controller);
                    Logger.Info($"Sleeping, run time = {DateTime.UtcNow.Subtract(startTime).TotalSeconds.ToString("0.00")}s");
                    controller.Sleep(1000 * 60 * 10);
                    Logger.Info("Done Sleeping");
                } catch (SwitchProcessingActionException exc)
                {
                    controller.Log("info", "switching action " + exc.Message);
                }
            }
        }
    }

}
