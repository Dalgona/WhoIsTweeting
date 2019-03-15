using System.Collections.Specialized;
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
            service.Graph.CollectionChanged += OnGraphCollectionChanged;
        }

        protected virtual void OnGraphCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("");
        }
    }
}
