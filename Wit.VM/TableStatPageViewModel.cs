namespace Wit.VM
{
    public class TableStatPageViewModel : StatPageViewModel
    {
        public override string DisplayName => StringProvider.GetString("Stat_Tab_Table") ?? "Table";

        public TableStatPageViewModel() : base() { }

        public void ResetStat()
        {
            service.ResetStatistics();
            OnPropertyChanged("");
        }
    }
}
