﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Wit.Core
{
    class StatManager
    {
        private int _sumOnline = 0;

        public ObservableCollection<StatData> Data { get; } = new ObservableCollection<StatData>();

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
                select g;

            Dictionary<UserStatus, int> counts = new Dictionary<UserStatus, int>()
            {
                { UserStatus.Online, 0 },
                { UserStatus.Away, 0 },
                { UserStatus.Offline, 0 }
            };

            foreach (var group in countQuery)
            {
                counts[group.Key] = group.Count();
            }

            OnlineCount = counts[UserStatus.Online];
            AwayCount = counts[UserStatus.Away];
            OfflineCount = counts[UserStatus.Offline];

            _sumOnline += OnlineCount;
            MinOnline = DataCount == 0 ? OnlineCount : (OnlineCount < MinOnline ? OnlineCount : MinOnline);
            MaxOnline = OnlineCount > MaxOnline ? OnlineCount : MaxOnline;

            lock (((ICollection)Data).SyncRoot)
            {
                Data.Add(new StatData(OnlineCount, AwayCount, OfflineCount));
            }
        }

        public void Reset()
        {
            lock (((ICollection)Data).SyncRoot)
            {
                Data.Clear();
            }

            _sumOnline = MinOnline = MaxOnline = 0;
        }
    }
}
