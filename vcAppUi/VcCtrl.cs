using ccVcontrol;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vcAppUi
{
    public class VcCtrl
    {
        public SimpleController controller = new SimpleController();
        ILog Logger = LogManager.GetLogger("main");
        public void Start()
        {
            new Thread(Run).Start();
        }
        protected void Run()
        {            
            while (true)
            {
                DateTime startTime = DateTime.UtcNow;
                Logger.Info("Starting");
                try
                {
                    controller.RefreshNetwork();
                    ccVcontrol.Program.Start(controller);
                    Logger.Info($"Sleeping, run time = {DateTime.UtcNow.Subtract(startTime).TotalSeconds.ToString("0.00")}s");
                    controller.Sleep(1000 * 60 * 20, true);
                    Logger.Info("Done Sleeping");
                }
                catch (SwitchProcessingActionException exc)
                {
                    controller.Log("info", "switching action " + exc.Message);
                }catch (Exception exc)
                {
                    controller.Log("error", "Error in vctrl " + exc.ToString());
                    controller.Sleep(1000 * 60 * 20, true);
                }
            }
        }
    }
}
