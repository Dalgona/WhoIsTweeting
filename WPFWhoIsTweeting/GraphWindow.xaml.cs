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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls.Primitives;

namespace WhoIsTweeting
{
    /// <summary>
    /// GraphWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GraphWindow : Window
    {
        private GraphViewModel viewModel;
        private MainService service = (Application.Current as App).Service;

        public GraphWindow()
        {
            InitializeComponent();

            DataContext = viewModel = new GraphViewModel();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
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
            Point pos = e.GetPosition(graphGrid);
            double segWidth = graphGrid.ActualWidth / viewModel.DataCount;
            int dataIndex = (int)Math.Round(pos.X / segWidth);
            cursor.X1 = cursor.X2 = Math.Floor(dataIndex * segWidth) - 0.5;
            peek.DataContext = service.Graph[dataIndex == 0 ? 0 : dataIndex - 1];
        }

        private void graphGrid_MouseEnter(object sender, MouseEventArgs e)
        { 
            cursor.Visibility = Visibility.Visible;
            peek.IsOpen = true;
        }

        private void graphGrid_MouseLeave(object sender, MouseEventArgs e)
        { 
            cursor.Visibility = Visibility.Hidden;
            peek.IsOpen = false;
        }
    }
}
