using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;

namespace WhoIsTweeting
{
    /// <summary>
    /// GraphWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GraphWindow : Window
    {
        private GraphViewModel viewModel;
        private MainService service = (Application.Current as App).Service;

        private DispatcherTimer timer;
        private int dataIndex;

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
                double y = 1 - ((service.Graph[i].Value[0] + service.Graph[i].Value[1]) / followings);
                pt.Add(new Point(x, y));
            }
            pt.Add(new Point(1, 1));
            pt = polyOnline.Points;
            pt.Clear();
            pt.Add(new Point(0, 1));
            for (int i = 0; i < num; i++)
            {
                double x = (i + 1.0) / num;
                double y = 1 - (service.Graph[i].Value[0] / followings);
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
    }
}
