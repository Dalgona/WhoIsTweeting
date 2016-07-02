﻿using PicoBird.Objects;
using System;

namespace WPFWhoIsTweeting
{
    public enum UserStatus { Online, Away, Offline };

    public class UserListItem
    {
        public static DateTime lastUpdated;

        public string ID { get; private set; }
        public string Name { get; private set; }
        public string ScreenName { get; private set; }
        public DateTime LastTweet { get; private set; }

        public int MinutesFromLastTweet
        {
            get
            {
                return (int)(lastUpdated - LastTweet).TotalMinutes;
            }
        }

        public UserStatus Status
        {
            get
            {
                return MinutesFromLastTweet <= 5 ? UserStatus.Online :
                    MinutesFromLastTweet <= 15 ? UserStatus.Away : UserStatus.Offline;
            }
        }

        public UserListItem(string id_str, string name, string screenName, Tweet lastTweet)
        {
            ID = id_str;
            Name = name;
            ScreenName = screenName;
            if (lastTweet == null) LastTweet = DateTime.FromBinary(0);
            else LastTweet = lastTweet.created_at.ToLocalTime();
        }
    }
}