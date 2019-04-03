using System;

namespace Wit.Twitter.Objects
{
    public interface ITwitterObject { }

    public class User : ITwitterObject
    {
        public bool? contributors_enabled { get; set; }
        public DateTime created_at { get; set; }
        public bool? default_profile { get; set; }
        public bool? default_profile_image { get; set; }
        public string description { get; set; }
        public Entity entities { get; set; }
        public int? favourites_count { get; set; }
        public bool? follow_request_sent { get; set; }
        public bool? following { get; set; }
        public int? followers_count { get; set; }
        public int? friends_count { get; set; }
        public bool? geo_enabled { get; set; }
        public long? id { get; set; }
        public string id_str { get; set; }
        public bool? is_translator { get; set; }
        public string lang { get; set; }
        public int? listed_count { get; set; }
        public string location { get; set; }
        public string name { get; set; }
        public bool? notifications { get; set; }
        public string profile_background_color { get; set; }
        public string profile_background_image_url { get; set; }
        public string profile_background_image_url_https { get; set; }
        public bool profile_background_tile { get; set; }
        public string profile_banner_url { get; set; }
        public string profile_image_url { get; set; }
        public string profile_image_url_https { get; set; }
        public string profile_link_color { get; set; }
        public string profile_sidebar_border_color { get; set; }
        public string profile_sidebar_fill_color { get; set; }
        public string profile_text_color { get; set; }
        public bool? profile_use_background_image { get; set; }
        public bool? @protected { get; set; }
        public string screen_name { get; set; }
        public bool? show_all_inline_media { get; set; }
        public Tweet status { get; set; }
        public int? statuses_count { get; set; }
        public string time_zone { get; set; }
        public string url { get; set; }
        public int? utc_offset { get; set; }
        public bool? verified { get; set; }
        public string withheld_in_countries { get; set; }
        public string withheld_scope { get; set; }
    }

    public class Tweet : ITwitterObject
    {
        public object annotations { get; set; }
        public object contributors { get; set; }
        public object coordinates { get; set; }
        public DateTime created_at { get; set; }
        public object current_user_retweet { get; set; }
        public Entity entities { get; set; }
        public int? favorite_count { get; set; }
        public bool? favorited { get; set; }
        public string filter_lever { get; set; }
        public object geo { get; set; }
        public long? id { get; set; }
        public string id_str { get; set; }
        public string in_reply_to_screen_name { get; set; }
        public long? in_reply_to_status_id { get; set; }
        public string in_reply_to_status_id_str { get; set; }
        public long? in_reply_to_user_id { get; set; }
        public string in_reply_to_user_id_str { get; set; }
        public string lang { get; set; }
        public object place { get; set; }
        public bool? possibly_sensitive { get; set; }
        public long? quoted_status_id { get; set; }
        public string quoted_status_id_str { get; set; }
        public Tweet quoted_status { get; set; }
        public object scope { get; set; }
        public int? retweet_count { get; set; }
        public bool? retweeted { get; set; }
        public Tweet retweeted_status { get; set; }
        public string source { get; set; }
        public string text { get; set; }
        public bool? truncated { get; set; }
        public User user { get; set; }
        public bool? withheld_copyright { get; set; }
        public string[] withheld_in_countries { get; set; }
        public string withheld_scope { get; set; }
    }

    public class DirectMessage : ITwitterObject
    {
        public DateTime created_at { get; set; }
        public Entity entities { get; set; }
        public long? id { get; set; }
        public string id_str { get; set; }
        public User recipient { get; set; }
        public long? recipient_id { get; set; }
        public string recipient_screen_name { get; set; }
        public User sender { get; set; }
        public long? sender_id { get; set; }
        public string sender_screen_name { get; set; }
        public string text { get; set; }
    }

    public class List : ITwitterObject
    {
        public long? id { get; set; }
        public string id_str { get; set; }
        public string name { get; set; }
        public string uri { get; set; }
        public long? subscriber_count { get; set; }
        public long? member_count { get; set; }
        public string mode { get; set; }
        public string description { get; set; }
        public string slug { get; set; }
        public string full_name { get; set; }
        public DateTime created_at { get; set; }
        public bool? following { get; set; }
        public User user { get; set; }
    }

    public class Entity : ITwitterObject
    {
        public class Hashtag
        {
            public int[] indices { get; set; }
            public string text { get; set; }
        }

        public class Media
        {
            public class Sizes
            {
                public class Size
                {
                    public int w { get; set; }
                    public int h { get; set; }
                    public string resize { get; set; }
                }

                public Size thumb { get; set; }
                public Size large { get; set; }
                public Size medium { get; set; }
                public Size small { get; set; }
            }

            public string display_url { get; set; }
            public string expanded_url { get; set; }
            public long? id { get; set; }
            public string id_str { get; set; }
            public int[] indices { get; set; }
            public string media_url { get; set; }
            public string media_url_https { get; set; }
            public Sizes sizes { get; set; }
            public long? source_status_id { get; set; }
            public string source_status_id_str { get; set; }
            public string type { get; set; }
            public string url { get; set; }
        }

        public class URL
        {
            public string display_url { get; set; }
            public string expanded_url { get; set; }
            public int[] indices { get; set; }
            public string url { get; set; }
        }

        public class UserMention
        {
            public long? id { get; set; }
            public string id_str { get; set; }
            public int[] indices { get; set; }
            public string name { get; set; }
            public string screen_name { get; set; }
        }

        public Hashtag[] hashtags { get; set; }
        public Media[] media { get; set; }
        public URL[] urls { get; set; }
        public UserMention[] user_mentions { get; set; }
    }

    public class Errors : ITwitterObject
    {
        public class Error : ITwitterObject
        {
            public int code { get; set; }
            public string message { get; set; }
        }

        public Error[] errors { get; set; }
    }

    public abstract class CursoredObjects : ITwitterObject
    {
        public long previous_cursor { get; set; }
        public long next_cursor { get; set; }
        public string previous_cursor_str { get; set; }
        public string next_cursor_str { get; set; }
    }

    public class CursoredIdStrings : CursoredObjects
    {
        public string[] ids { get; set; }
    }

    public class CursoredLists : CursoredObjects
    {
        public List[] lists { get; set; }
    }
}
