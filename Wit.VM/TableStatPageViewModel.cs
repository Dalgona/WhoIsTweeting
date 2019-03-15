using Wit.UI.Core;

namespace Wit.VM
{
    public class TableStatPageViewModel : StatPageViewModel
    {
        public override string DisplayName => "Table"; // TODO: Replace with a localized string.

        public TableStatPageViewModel() : base() { }
        public TableStatPageViewModel(ViewModelFactory vmFactory, IWindowManager winManager) : base(vmFactory, winManager) { }

        public void ResetStat()
        {
            service.ResetStatistics();
            OnPropertyChanged("");
        }
    }
}
