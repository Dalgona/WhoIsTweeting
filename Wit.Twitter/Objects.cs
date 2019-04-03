using System;

namespace Wit.Twitter.Objects
{
    public interface ITwitterObject { }

    public class User : ITwitterObject
    {
        public bool? ContributorsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? DefaultProfile { get; set; }
        public bool? DefaultProfileImage { get; set; }
        public string Description { get; set; }
        public Entity Entities { get; set; }
        public int? FavouritesCount { get; set; }
        public bool? FollowRequestSent { get; set; }
        public bool? Following { get; set; }
        public int? FollowersCount { get; set; }
        public int? FriendsCount { get; set; }
        public bool? GeoEnabled { get; set; }
        public long? Id { get; set; }
        public string IdStr { get; set; }
        public bool? IsTranslator { get; set; }
        public string Lang { get; set; }
        public int? ListedCount { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public bool? Notifications { get; set; }
        public string ProfileBackgroundColor { get; set; }
        public string ProfileBackgroundImageUrl { get; set; }
        public string ProfileBackgroundImageUrlHttps { get; set; }
        public bool ProfileBackgroundTile { get; set; }
        public string ProfileBannerUrl { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ProfileImageUrlHttps { get; set; }
        public string ProfileLinkColor { get; set; }
        public string ProfileSidebarBorderColor { get; set; }
        public string ProfileSidebarFillColor { get; set; }
        public string ProfileTextColor { get; set; }
        public bool? ProfileUseBackgroundImage { get; set; }
        public bool? Protected { get; set; }
        public string ScreenName { get; set; }
        public bool? ShowAllInlineMedia { get; set; }
        public Tweet Status { get; set; }
        public int? StatusesCount { get; set; }
        public string TimeZone { get; set; }
        public string Url { get; set; }
        public int? UtcOffset { get; set; }
        public bool? Verified { get; set; }
        public string WithheldInCountries { get; set; }
        public string WithheldScope { get; set; }
    }

    public class Tweet : ITwitterObject
    {
        public object Annotations { get; set; }
        public object Contributors { get; set; }
        public object Coordinates { get; set; }
        public DateTime CreatedAt { get; set; }
        public object CurrentUserRetweet { get; set; }
        public Entity Entities { get; set; }
        public int? FavoriteCount { get; set; }
        public bool? Favorited { get; set; }
        public string FilterLever { get; set; }
        public object Geo { get; set; }
        public long? Id { get; set; }
        public string IdStr { get; set; }
        public string InReplyToScreenName { get; set; }
        public long? InReplyToStatusId { get; set; }
        public string InReplyToStatusIdStr { get; set; }
        public long? InReplyToUserId { get; set; }
        public string InReplyToUserIdStr { get; set; }
        public string Lang { get; set; }
        public object Place { get; set; }
        public bool? PossiblySensitive { get; set; }
        public long? QuotedStatusId { get; set; }
        public string QuotedStatusIdStr { get; set; }
        public Tweet QuotedStatus { get; set; }
        public object Scope { get; set; }
        public int? RetweetCount { get; set; }
        public bool? Retweeted { get; set; }
        public Tweet RetweetedStatus { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
        public bool? Truncated { get; set; }
        public User User { get; set; }
        public bool? WithheldCopyright { get; set; }
        public string[] WithheldInCountries { get; set; }
        public string WithheldScope { get; set; }
    }

    public class DirectMessage : ITwitterObject
    {
        public DateTime CreatedAt { get; set; }
        public Entity Entities { get; set; }
        public long? Id { get; set; }
        public string IdStr { get; set; }
        public User Recipient { get; set; }
        public long? RecipientId { get; set; }
        public string RecipientScreenName { get; set; }
        public User Sender { get; set; }
        public long? SenderId { get; set; }
        public string SenderScreenName { get; set; }
        public string Text { get; set; }
    }

    public class List : ITwitterObject
    {
        public long? Id { get; set; }
        public string IdStr { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public long? SubscriberCount { get; set; }
        public long? MemberCount { get; set; }
        public string Mode { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? Following { get; set; }
        public User User { get; set; }
    }

    public class Hashtag
    {
        public int[] Indices { get; set; }
        public string Text { get; set; }
    }

    public class Sizes
    {
        public class Size
        {
            public int W { get; set; }
            public int H { get; set; }
            public string Resize { get; set; }
        }

        public Size Thumb { get; set; }
        public Size Large { get; set; }
        public Size Medium { get; set; }
        public Size Small { get; set; }
    }

    public class Media
    {
        public string DisplayUrl { get; set; }
        public string ExpandedUrl { get; set; }
        public long? Id { get; set; }
        public string IdStr { get; set; }
        public int[] Indices { get; set; }
        public string MediaUrl { get; set; }
        public string MediaUrlHttps { get; set; }
        public Sizes Sizes { get; set; }
        public long? SourceStatusId { get; set; }
        public string SourceStatusIdStr { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }

    public class URL
    {
        public string DisplayUrl { get; set; }
        public string ExpandedUrl { get; set; }
        public int[] Indices { get; set; }
        public string Url { get; set; }
    }

    public class UserMention
    {
        public long? Id { get; set; }
        public string IdStr { get; set; }
        public int[] Indices { get; set; }
        public string Name { get; set; }
        public string ScreenName { get; set; }
    }

    public class Entity : ITwitterObject
    {
        public Hashtag[] Hashtags { get; set; }
        public Media[] Media { get; set; }
        public URL[] Urls { get; set; }
        public UserMention[] UserMentions { get; set; }
    }

    public class ApiErrors : ITwitterObject
    {
        public class ApiError : ITwitterObject
        {
            public int Code { get; set; }
            public string Message { get; set; }
        }

        public ApiError[] Errors { get; set; }
    }

    public abstract class CursoredObjects : ITwitterObject
    {
        public long PreviousCursor { get; set; }
        public long NextCursor { get; set; }
        public string PreviousCursorStr { get; set; }
        public string NextCursorStr { get; set; }
    }

    public class CursoredIdStrings : CursoredObjects
    {
        public string[] Ids { get; set; }
    }

    public class CursoredLists : CursoredObjects
    {
        public List[] Lists { get; set; }
    }
}
