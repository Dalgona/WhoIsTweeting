using System;

namespace Wit.Core
{
    public class StatData
    {
        public DateTime Date { get; }
        public int OnlineCount { get; }
        public int AwayCount { get; }
        public int OfflineCount { get; }

        public StatData(int online, int away, int offline)
        {
            Date = DateTime.Now;
            OnlineCount = online;
            AwayCount = away;
            OfflineCount = offline;
        }
    }
}
