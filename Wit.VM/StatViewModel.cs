﻿using System.Collections;
using System.ComponentModel;
using System.Linq;
using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public class StatViewModel : WindowViewModel
    {
        private const string dateformat = "yyyy-MM-dd HH:mm";
        private MainService service = MainService.Instance;

        public StatViewModel()
        {
            service.PropertyChanged += Service_PropertyChanged;

            Width = 450;
            Height = 500;
            MinWidth = 450;
            MinHeight = 500;
        }

        public StatViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : this()
        {
            this.vmFactory = vmFactory;
            this.winManager = winManager;
        }

        private void Service_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State" && service.State == ServiceState.Running)
            {
                OnPropertyChanged("");
            }
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

        public IEnumerable Graph => service.Graph;
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
    }
}