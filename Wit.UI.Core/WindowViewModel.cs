namespace Wit.UI.Core
{
    public class WindowViewModel : ViewModelBase
    {
        private string _title = string.Empty;
        private double _width = double.NaN;
        private double _height = double.NaN;
        private double _minWidth = 0.0;
        private double _maxWidth = double.PositiveInfinity;
        private double _minHeight = 0.0;
        private double _maxHeight = double.PositiveInfinity;
        private bool _canResize = true;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        public double MinWidth
        {
            get => _minWidth;
            set
            {
                _minWidth = value;
                OnPropertyChanged(nameof(MinWidth));
            }
        }

        public double MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                OnPropertyChanged(nameof(MaxWidth));
            }
        }

        public double MinHeight
        {
            get => _minHeight;
            set
            {
                _minHeight = value;
                OnPropertyChanged(nameof(MinHeight));
            }
        }

        public double MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = value;
                OnPropertyChanged(nameof(MaxHeight));
            }
        }

        public bool CanResize
        {
            get => _canResize;
            set
            {
                _canResize = value;
                OnPropertyChanged(nameof(CanResize));
            }
        }
    }
}
