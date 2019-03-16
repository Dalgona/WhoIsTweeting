using System.Collections.Generic;
using System.Linq;
using Wit.UI.Core;

namespace Wit.VM
{
    public class StatViewModel : WindowViewModel
    {
        private StatPageViewModel _selectedPage;

        public override string Title => StringProvider?.GetString("Stat_Title") ?? "Statistics";

        public IEnumerable<StatPageViewModel> Pages { get; } =
            new StatPageViewModel[]
            {
                DepsInjector.Default.Create<GraphStatPageViewModel>(),
                DepsInjector.Default.Create<TableStatPageViewModel>()
            };

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

            SelectedPage = Pages.First();
        }
    }
}
