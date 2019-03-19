﻿using System.Collections.Specialized;
using System.ComponentModel;
using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public abstract class StatPageViewModel : ViewModelBase
    {
        protected readonly MainService service;

        public abstract string DisplayName { get; }

        public virtual int DataCount => service.GraphCount;
        public virtual INotifyCollectionChanged Graph => service.Graph;

        public StatPageViewModel()
        {
            service = MainService.Instance;
            service.PropertyChanged += OnServicePropertyChanged;
            service.Graph.CollectionChanged += OnGraphCollectionChanged;
        }

        protected virtual void OnServicePropertyChanged(object sender, PropertyChangedEventArgs e)
            => OnPropertyChanged("");

        protected virtual void OnGraphCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged("");
    }
}
