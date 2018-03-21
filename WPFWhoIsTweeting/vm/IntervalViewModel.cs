using System.ComponentModel;
using System.Windows;

namespace WhoIsTweeting
{
    public class IntervalViewModel : INotifyPropertyChanged
    {
        public int Interval
        {
            get => interval;
            set
            {
                if (value <= 10) interval = 10;
                else if (value >= 60) interval = 60;
                else interval = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Interval"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxFollowings"));
            }
        }
        public int MaxFollowings => Interval * 20;

        private MainService service;
        private int interval;

        public event PropertyChangedEventHandler PropertyChanged;

        public IntervalViewModel()
        {
            service = (Application.Current as App).Service;
            Interval = service.UpdateInterval;
        }

        public void CommitInterval() => service.UpdateInterval = interval;
    }
}
