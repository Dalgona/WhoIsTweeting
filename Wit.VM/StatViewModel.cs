using System.Collections.Generic;
using System.Linq;
using Wit.UI.Core;

namespace Wit.VM
{
    public class StatViewModel : WindowViewModel
    {
        private StatPageViewModel _selectedPage;

        public IEnumerable<StatPageViewModel> Pages { get; }

        public StatPageViewModel SelectedPage
        {
            get => _selectedPage;
            set
            {
                _selectedPage = value;
                OnPropertyChanged(nameof(SelectedPage));
            }
        }

        public StatViewModel()
        {
            Width = 450;
            Height = 500;
            MinWidth = 450;
            MinHeight = 500;
        }

        public StatViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : this()
        {
            Pages = new StatPageViewModel[]
            {
                (GraphStatPageViewModel)vmFactory.Create<GraphStatPageViewModel>(),
                (TableStatPageViewModel)vmFactory.Create<TableStatPageViewModel>()
            };

            SelectedPage = Pages.First();

            this.vmFactory = vmFactory;
            this.winManager = winManager;
        }
    }
}
