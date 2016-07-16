using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WhoIsTweeting
{
    public class IntervalViewModel : INotifyPropertyChanged
    {
        public int Interval
        {
            get { return interval; }
            set
            {
                if (value <= 10) interval = 10;
                else if (value >= 60) interval = 60;
                else interval = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Interval"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxFollowings"));
            }
        }
        public int MaxFollowings { get { return Interval * 20; } }

        private MainService service;
        private int interval;

        public event PropertyChangedEventHandler PropertyChanged;

        public IntervalViewModel()
        {
            service = (Application.Current as App).Service;
            Interval = service.UpdateInterval;
        }

        public void CommitInterval()
            => service.UpdateInterval = interval;
    }
}
