using System.Linq;

namespace Wit.VM
{
    public class GraphStatPageViewModel : StatPageViewModel
    {
        private const string _dateFormat = "yyyy-MM-dd HH:mm";

        public override string DisplayName => StringProvider.GetString("Stat_Tab_Graph") ?? "Graph";

        public string From
        {
            get
            {
                if (service.StatData.Count == 0) return "N/A";
                return service.StatData[0].Date.ToString(_dateFormat);
            }
        }

        public string To
        {
            get
            {
                if (service.StatData.Count == 0) return "N/A";
                return service.StatData.Last().Date.ToString(_dateFormat);
            }
        }

        public int MinOnline => service.MinOnline;
        public int MaxOnline => service.MaxOnline;
        public double AvgOnline => service.AvgOnline;

        public GraphStatPageViewModel() : base() { }
    }
}
