﻿using System;
using PicoBird.Objects;

namespace Wit.Core
{
    public enum UserStatus { Online, Away, Offline };

    public class UserListItem
    {
        public static DateTime lastUpdated;

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string ScreenName { get; private set; }
        public DateTime LastTweet { get; private set; }

        public int MinutesFromLastTweet => (int)(lastUpdated - LastTweet).TotalMinutes;

        public UserStatus Status
            => MinutesFromLastTweet <= 5 ? UserStatus.Online
                : MinutesFromLastTweet <= 15 ? UserStatus.Away : UserStatus.Offline;

        public UserListItem(string idStr, string name, string screenName, Tweet lastTweet)
        {
            Id = idStr;
            Name = name;
            ScreenName = screenName;
            if (lastTweet == null) LastTweet = DateTime.FromBinary(0);
            else LastTweet = lastTweet.created_at.ToLocalTime();
        }
    }
}
