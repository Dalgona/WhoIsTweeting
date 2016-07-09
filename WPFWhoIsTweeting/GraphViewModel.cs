using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Windows.Data;

namespace WhoIsTweeting
{
    class GraphViewModel : INotifyPropertyChanged
    {
        private static readonly string dateformat = "yyyy-MM-dd HH:mm";
        private MainService service = (Application.Current as App).Service;

        public GraphViewModel()
        {
            service.PropertyChanged += Service_PropertyChanged;
        }

        private void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State" && service.State == ServiceState.Running)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public string From
        {
            get
            {
                if (service.Graph.Count == 0) return "N/A";
                return service.Graph[0].Key.ToString(dateformat);
            }
        }

        public string To
        {
            get
            {
                if (service.Graph.Count == 0) return "N/A";
                return service.Graph.Last().Key.ToString(dateformat);
            }
        }

        public int DataCount { get { return service.GraphCount; } }
        public int FollowingsCount { get { return service.OnlineCount + service.AwayCount + service.OfflineCount; } }

        public int MinOnline { get { return service.MinOnline; } }
        public int MaxOnline { get { return service.MaxOnline; } }
        public double AvgOnline { get { return service.SumOnline / (double)service.GraphCount; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
