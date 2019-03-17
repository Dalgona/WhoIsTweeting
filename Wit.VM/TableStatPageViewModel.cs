using Wit.UI.Core;

namespace Wit.VM
{
    public class TableStatPageViewModel : StatPageViewModel
    {
        private RelayCommand _resetStatCommand;

        public override string DisplayName => StringProvider.GetString("Stat_Tab_Table") ?? "Table";

        public RelayCommand ResetStatCommand
            => _resetStatCommand ?? (_resetStatCommand = new RelayCommand(ResetStatExecuted));

        public TableStatPageViewModel() : base() { }

        private void ResetStatExecuted()
        {
            string title = StringProvider.GetString("Stat_Reset_Title");
            string message = StringProvider.GetString("Stat_Reset_Message");

            if (MessageBoxHelper.ShowYesNo(title, message))
            {
                service.ResetStatistics();
                OnPropertyChanged("");
            }
        }
    }
}
