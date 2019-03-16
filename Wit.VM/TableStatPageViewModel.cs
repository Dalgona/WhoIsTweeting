namespace Wit.VM
{
    public class TableStatPageViewModel : StatPageViewModel
    {
        public override string DisplayName => "Table"; // TODO: Replace with a localized string.

        public TableStatPageViewModel() : base() { }

        public void ResetStat()
        {
            service.ResetStatistics();
            OnPropertyChanged("");
        }
    }
}
