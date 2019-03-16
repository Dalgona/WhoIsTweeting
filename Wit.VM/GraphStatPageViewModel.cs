using System.Linq;

namespace Wit.VM
{
    public class GraphStatPageViewModel : StatPageViewModel
    {
        private const string _dateFormat = "yyyy-MM-dd HH:mm";

        public override string DisplayName => "Graph"; // TODO: Replace with a localized string.

        public string From
        {
            get
            {
                if (service.Graph.Count == 0) return "N/A";
                return service.Graph[0].Date.ToString(_dateFormat);
            }
        }

        public string To
        {
            get
            {
                if (service.Graph.Count == 0) return "N/A";
                return service.Graph.Last().Date.ToString(_dateFormat);
            }
        }

        public int MinOnline => service.MinOnline;
        public int MaxOnline => service.MaxOnline;
        public double AvgOnline => service.SumOnline / (double)service.GraphCount;

        public GraphStatPageViewModel() : base() { }
    }
}
