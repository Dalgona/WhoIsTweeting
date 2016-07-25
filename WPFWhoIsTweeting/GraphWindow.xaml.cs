using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Text;
using Newtonsoft.Json;

namespace WhoIsTweeting
{
    public partial class GraphWindow : Window
    {
        private GraphViewModel viewModel;
        private MainService service = (Application.Current as App).Service;

        private DispatcherTimer timer;
        private int dataIndex;

        private double graphScale = 1.0;

        public GraphWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new GraphViewModel();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            peek.DataContext = service.Graph[dataIndex == 0 ? 0 : dataIndex - 1];
            peek.IsOpen = true;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => Dispatcher.Invoke(drawGraph);

        private void drawGraph()
        {
            int num = viewModel.DataCount;
            double followings = viewModel.FollowingsCount;
            var pt = polyAway.Points;
            pt.Clear();
            pt.Add(new Point(0, 1));
            for (int i = 0; i < num; i++)
            {
                double x = (i + 1.0) / num;
                double y = 1 - ((service.Graph[i].Value[0] + service.Graph[i].Value[1]) / followings) * graphScale;
                pt.Add(new Point(x, y));
            }
            pt.Add(new Point(1, 1));
            pt = polyOnline.Points;
            pt.Clear();
            pt.Add(new Point(0, 1));
            for (int i = 0; i < num; i++)
            {
                double x = (i + 1.0) / num;
                double y = 1 - (service.Graph[i].Value[0] / followings) * graphScale;
                pt.Add(new Point(x, y));
            }
            pt.Add(new Point(1, 1));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            list1.ItemsSource = service.Graph;
            drawGraph();
        }

        private void MoveCursor(object sender, MouseEventArgs e)
        {
            if (viewModel.DataCount == 0) return;
            timer.Stop();
            peek.IsOpen = false;
            Point pos = e.GetPosition(graphGrid);
            double segWidth = graphGrid.ActualWidth / viewModel.DataCount;
            dataIndex = (int)Math.Round(pos.X / segWidth);
            cursor.X1 = cursor.X2 = Math.Floor(dataIndex * segWidth) - 0.5;
            timer.Start();
        }

        private void graphGrid_MouseEnter(object sender, MouseEventArgs e)
        { 
            cursor.Visibility = Visibility.Visible;
        }

        private void graphGrid_MouseLeave(object sender, MouseEventArgs e)
        { 
            cursor.Visibility = Visibility.Hidden;
            peek.IsOpen = false;
            timer.Stop();
        }

        private void graphGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && graphScale <= 5.0) graphScale += 0.1;
            if (e.Delta < 0 && graphScale >= 1.0) graphScale -= 0.1;
            drawGraph();
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                Application.Current.FindResource("Statistics_Reset_Message").ToString(),
                Application.Current.FindResource("Statistics_Reset_Title").ToString(),
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes) viewModel.ResetStat();
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = Application.Current.FindResource("Statistics_Export_Title").ToString();
            dlg.Filter = Application.Current.FindResource("Statistics_Export_Filter").ToString();
            dlg.FileOk += OnExportDlgOK;
            dlg.ShowDialog();
        }

        private void OnExportDlgOK(object sender, CancelEventArgs e)
        {
            SaveFileDialog dlg = sender as SaveFileDialog;
            string extension = dlg.FileName.Split('.').Last().ToLower();
            if (extension != "csv" && extension != "json")
                MessageBox.Show(
                    Application.Current.FindResource("Statistics_Export_InvalidType").ToString(),
                    Application.Current.FindResource("Title_Error").ToString(),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            try
            {
                FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
                BufferedStream bfs = new BufferedStream(fs, 1024);
                byte[] utfstr;
                switch (extension)
                {
                    case "csv":
                        foreach (var x in service.Graph)
                        {
                            var arr = x.Value;
                            string date = x.Key.ToString("yyyy-MM-dd HH:mm:ss");
                            utfstr = Encoding.UTF8.GetBytes($"\"{date}\",{arr[0]},{arr[1]},{arr[2]}{Environment.NewLine}");
                            fs.Write(utfstr, 0, utfstr.Length);
                        }
                        break;
                    case "json":
                        string jsondat = JsonConvert.SerializeObject(
                            from x in service.Graph orderby x.Key ascending select x, Formatting.Indented);
                        utfstr = Encoding.UTF8.GetBytes(jsondat);
                        fs.Write(utfstr, 0, utfstr.Length);
                        break;
                }
                bfs.Flush();
                bfs.Close();
                fs.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    string.Format(Application.Current.FindResource("Statistics_Export_Exception").ToString(), ex.Message),
                    Application.Current.FindResource("Title_Error").ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
