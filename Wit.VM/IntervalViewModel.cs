using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public class IntervalViewModel : ViewModelBase
    {
        public int Interval
        {
            get => interval;
            set
            {
                if (value <= 10) interval = 10;
                else if (value >= 60) interval = 60;
                else interval = value;

                OnPropertyChanged(nameof(Interval));
                OnPropertyChanged(nameof(MaxFollowings));
            }
        }
        public int MaxFollowings => Interval * 20;

        private MainService service;
        private int interval;

        public IntervalViewModel()
        {
            service = MainService.Instance;
            Interval = service.UpdateInterval;
        }

        public IntervalViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : this()
        {
            this.vmFactory = vmFactory;
            this.winManager = winManager;
        }

        public void CommitInterval() => service.UpdateInterval = interval;
    }
}
