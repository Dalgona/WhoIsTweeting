using Wit.Core;
using Wit.UI.Core;

namespace Wit.VM
{
    public class IntervalViewModel : WindowViewModel
    {
        private readonly MainService _service;
        private readonly int _oldInterval;
        private int _interval;

        private RelayCommand _decreaseIntervalCommand;
        private RelayCommand _increaseIntervalCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;

        public int Interval
        {
            get => _interval;
            set
            {
                if (value <= 10) _interval = 10;
                else if (value >= 60) _interval = 60;
                else _interval = value;

                OnPropertyChanged(nameof(Interval));
                OnPropertyChanged(nameof(MaxFollowings));
            }
        }

        public RelayCommand DecreaseIntervalCommand
            => _decreaseIntervalCommand ?? (_decreaseIntervalCommand = new RelayCommand(() => Interval -= 5, () => Interval > 10));

        public RelayCommand IncreaseIntervalCommand
            => _increaseIntervalCommand ?? (_increaseIntervalCommand = new RelayCommand(() => Interval += 5, () => Interval < 60));

        public RelayCommand SaveCommand
            => _saveCommand ?? (_saveCommand = new RelayCommand(() =>
            {
                if (Interval != _oldInterval)
                {
                    _service.UpdateInterval = _interval;
                }

                WindowManager.CloseWindow(this);
            }));

        public RelayCommand CancelCommand
            => _cancelCommand ?? (_cancelCommand = new RelayCommand(() => WindowManager.CloseWindow(this)));

        public int MaxFollowings => Interval * 20;

        public IntervalViewModel()
        {
            _service = MainService.Instance;
            _oldInterval = Interval = _service.UpdateInterval;

            Width = 340;
            Height = 200;
            CanResize = false;
        }
    }
}
