using System;
using Wit.Twitter.Objects;

namespace Wit.Core
{
    public enum UserStatus { Online, Away, Offline };

    public class UserListItem
    {
        private static readonly DateTime _zeroDateTime = new DateTime(0L).ToLocalTime();

        public static DateTime LastUpdated { get; internal set; }

        public string Id { get; }
        public string Name { get; internal set; }
        public string ScreenName { get; internal set; }
        public DateTime LastTweet { get; internal set; }

        public int MinutesFromLastTweet => (int)(LastUpdated - LastTweet).TotalMinutes;

        public UserStatus Status
            => MinutesFromLastTweet <= 5 ? UserStatus.Online
                : MinutesFromLastTweet <= 15 ? UserStatus.Away : UserStatus.Offline;

        public UserListItem(string idStr, string name, string screenName, Tweet lastTweet)
        {
            Id = idStr;
            Name = name;
            ScreenName = screenName;
            LastTweet = lastTweet?.CreatedAt.ToLocalTime() ?? _zeroDateTime;
        }
    }
}
