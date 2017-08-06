using ccInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ccUi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtName.ItemsSource = new List<String> { "GoldMine", "ElixirCollector", "GoldStorage", "ElixirStorage", "Townhall", "Barracks" };
        }

        List<PosInfo> points = new List<PosInfo>();
        class GPInfo
        {
            public PosInfo pinfo;
            public Ellipse fill;  
        }

        string curFileName = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                points.Clear();
                gpInfo.Clear();
                canvas.Children.Clear();
                // Open document 
                string filename = dlg.FileName;
                curFileName = filename;
                //Convert Bitmap To Image                
                Image i = new Image();
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(filename, UriKind.Relative);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                i.Source = src;
                i.Stretch = Stretch.Uniform;                
                canvas.Children.Clear();
                canvas.Children.Add(i);
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            info.Text = e.GetPosition(canvas).X+ "," +e.GetPosition(canvas).Y;
        }

        List<GPInfo> gpInfo = new List<GPInfo>();
        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var data = new PosInfo { name = txtName.Text, point = new ccPoint((int)e.GetPosition(canvas).X, (int)e.GetPosition(canvas).Y) };
            if (points.Any(p =>
            {
                return Math.Abs(p.point.x - data.point.x) < 10
                && Math.Abs(p.point.y - data.point.y) < 10;
            })) return;
            points.Add(data);            
            Ellipse ele = new Ellipse();
            ele.Width = 10;
            ele.Height = 10;
            ele.Fill = Brushes.Aqua;
            canvas.Children.Add(ele);
            Canvas.SetLeft(ele, data.point.x -  (ele.Width / 2));
            Canvas.SetTop(ele, data.point.y - (ele.Height / 2));
            ele.MouseLeftButtonUp += Ele_MouseLeftButtonUp;
            var store = new GPInfo { pinfo = data, fill = ele };
            gpInfo.Add(store);
        }

        private void Ele_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var found = gpInfo.FirstOrDefault(itm => itm.fill == sender);
            if (found != null)
            {
                canvas.Children.Remove(found.fill);
                gpInfo.Remove(found);
                points.Remove(found.pinfo);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var fname = curFileName.Replace(".png", ".txt");
            File.WriteAllText(fname, JsonConvert.SerializeObject(points, Formatting.Indented));
        }
    }
}
