using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Wit.Core
{
    class StatManager
    {
        private int _sumOnline = 0;

        public ObservableCollection<StatData> Data { get; } = new ObservableCollection<StatData>();
        public object SyncRoot => ((ICollection)Data).SyncRoot;

        public int DataCount => Data.Count;
        public int OnlineCount { get; private set; } = 0;
        public int AwayCount { get; private set; } = 0;
        public int OfflineCount { get; private set; } = 0;

        public int MinOnline { get; private set; } = 0;
        public int MaxOnline { get; private set; } = 0;
        public double AvgOnline => _sumOnline / (double)DataCount;

        public void Update(IEnumerable<UserListItem> users)
        {
            var countQuery =
                from user in users
                group user by user.Status into g
                orderby g.Key
                select g.Count();

            List<int> counts = new List<int>(countQuery);
            OnlineCount = counts[0];
            AwayCount = counts[1];
            OfflineCount = counts[2];

            _sumOnline += OnlineCount;
            MinOnline = DataCount == 0 ? OnlineCount : (OnlineCount < MinOnline ? OnlineCount : MinOnline);
            MaxOnline = OnlineCount > MaxOnline ? OnlineCount : MaxOnline;

            lock (SyncRoot)
            {
                Data.Add(new StatData(OnlineCount, AwayCount, OfflineCount));
            }
        }

        public void Reset()
        {
            lock (SyncRoot)
            {
                Data.Clear();
            }

            MinOnline = MaxOnline = 0;
        }
    }
}
