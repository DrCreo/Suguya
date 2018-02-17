using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Suguya.Models
{
    [JsonObject]
    public class PostChannel
    {
        [JsonProperty("channel-id")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("filtered-tags")]
        public List<string> FilteredTags { get; private set; }

        [JsonProperty("filtered-sources")]
        public List<string> FilteredSources { get; private set; }

        [JsonProperty("post-toggle")]
        public bool PostToggle { get; private set; }

        [JsonProperty("minutes-between-posts")]
        public int MinutesBetweenPosts { get; private set; }

        [JsonProperty("rating")]
        public Rating Rating { get; private set; }

        [JsonProperty("allow-gif")]
        public bool AllowGif { get; private set; }

        [JsonProperty("embed-color-hex")]
        public string EmbedColorHex { get; private set; }

        [JsonIgnore]
        public DateTime NextPostTime { get; private set; }

        [JsonIgnore]
        public List<Post> Posts { get; private set; }

        public PostChannel(ulong channelId, List<string> filteredTags, List<string> filteredSources, bool postToggle, int minutesBetweenPosts = 5, Rating rating = Rating.S, bool allowGif = false, string embedColorHex = "9e8cb9")
        {
            ChannelId = channelId;
            FilteredTags = filteredTags;
            FilteredSources = filteredSources;
            PostToggle = postToggle;
            MinutesBetweenPosts = minutesBetweenPosts;
            Rating = rating;
            NextPostTime = DateTime.Now.AddMinutes(-MinutesBetweenPosts);
            AllowGif = false;
            EmbedColorHex = embedColorHex;
            Posts = new List<Post>();
        }

        public void SetPosts(List<Post> posts)
        {
            NextPostTime = DateTime.Now.AddMinutes(-MinutesBetweenPosts);
            AllowGif = false;
            Posts = posts;
        }

        public PostChannel()
        {
            Posts = new List<Post>();
        }

        public void UpdateNextPostTime()
        {
            NextPostTime = DateTime.Now.AddMinutes(MinutesBetweenPosts);
        }

        public void RemovePost(Post post)
        {
            Posts.Remove(post);
        }

        public static PostChannel GetExampleChannel()
        {
            return new PostChannel(000000000000, new List<string>(), new List<string>(), true);
        }

        public void SetTagFilter(List<string> taglist)
        {
            FilteredTags = taglist;
        }

        internal void SetRating(Rating rating)
        {
            Rating = rating;
        }
    }
}
