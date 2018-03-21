using System.Linq;
using System.ComponentModel;
using System.Windows;

namespace WhoIsTweeting
{
    class GraphViewModel : INotifyPropertyChanged
    {
        private const string dateformat = "yyyy-MM-dd HH:mm";
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

        public int DataCount => service.GraphCount;
        public int FollowingsCount => service.OnlineCount + service.AwayCount + service.OfflineCount;

        public int MinOnline => service.MinOnline;
        public int MaxOnline => service.MaxOnline;
        public double AvgOnline => service.SumOnline / (double)service.GraphCount;

        public void ResetStat()
        {
            service.ResetStatistics();
            OnPropertyChanged("");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
