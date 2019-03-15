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

        public StatPageViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : this()
        {
            this.vmFactory = vmFactory;
            this.winManager = winManager;
        }

        protected virtual void OnGraphCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("");
        }
    }
}
