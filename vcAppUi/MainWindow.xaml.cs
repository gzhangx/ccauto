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
            ctrl.Start();
        }

        private void Button_Click_SwitchAccount(object sender, RoutedEventArgs e)
        {
            var act = ((ComboBoxItem)cmbAccount.SelectedValue).Content.ToString();
            ctrl.controller.ChangeToNewAccount(Int32.Parse(act));
        }
    }
}
