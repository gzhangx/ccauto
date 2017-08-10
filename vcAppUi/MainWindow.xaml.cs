using ccVcontrol;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vcAppUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ILog Logger = LogManager.GetLogger("main");
        VcCtrl ctrl = new VcCtrl();
        public MainWindow()
        {            
            InitializeComponent();
            //ctrl.Start();
            ctrl.controller.EventNotify = (what, val) =>
            {
                Logger.Info($"UI: {what} ==> {val}");
                switch (what)
                {
                    case "startingAccount":
                        this.Dispatcher.BeginInvoke(new Action(()=> {
                            cmbAccount.SelectedIndex = Int32.Parse(val) - 1;
                        }));
                        break;
                }
            };
        }

        private void Button_Click_SwitchAccount(object sender, RoutedEventArgs e)
        {
            var act = ((ComboBoxItem)cmbAccount.SelectedValue).Content.ToString();
            ctrl.controller.ChangeToNewAccount(Int32.Parse(act) - 1);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PcWakeup.SetWaitForWakeUpTime(ctrl.controller, 10000, true);
            
        }
    }
}
