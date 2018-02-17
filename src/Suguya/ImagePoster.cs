using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Suguya.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Suguya
{
    public class ImagePoster
    {
        public List<PostChannel> Channels { get; private set; }

        private DiscordClient _client { get; set; }

        private Random rng = new Random();
        private string ChannelDataFilePath = "channels.json";
        private bool doPost = false;


        /// <summary>
        /// Starts the main loop used for posting.
        /// </summary>
        /// <returns></returns>
        public async Task StartPosting()
        {
            await LoadChannelData();


            // Main loop for posting.
            while (doPost)
            {
                if (Channels.Count == 0)
                {
                    await Task.Delay(5000);
                    continue;
                }
                foreach (var channel in Channels)
                    if (channel.NextPostTime < DateTime.Now && channel.PostToggle)
                        await PostToChannel(channel);

                await Task.Delay(5000);
            }
        }

        /// <summary>
        /// Posts a random post to the channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private async Task PostToChannel(PostChannel channel)
        {
            if (channel.Posts == null)
                channel.SetPosts(await PopulateChannelPostsAsync(channel));

            if (channel.Posts.Count == 0)
                channel.SetPosts(await PopulateChannelPostsAsync(channel));

            var discordChannel = await _client.GetChannelAsync(channel.ChannelId);

            var i = true;
            while (i)
            {

                // fail safe incase we remove all posts for some reason and end up with non.
                if (channel.Posts.Count == 0)
                    channel.SetPosts(await PopulateChannelPostsAsync(channel));

                var post = channel.Posts[rng.Next(0, channel.Posts.Count - 1)];

                // filter tags and sources
                if (post.Tags.Any(t => channel.FilteredTags.Contains(t)) || channel.FilteredSources.Contains(post.Source))
                {
                    channel.RemovePost(post);
                    continue;
                }

                // filter webm, Filter gif if the channel set AllowGif to false
                if (post.Extension == "webm" || (post.Extension == "gif" && !channel.AllowGif))
                {
                    channel.RemovePost(post);
                    continue;
                }

                var contentWarning = "(Warning website may NSFW content)";
                // build the embed
                var eb = new DiscordEmbedBuilder
                {
                    // if the post is meant to be safe show a warning in the title.
                    Title = $"{post.Source}" + (post.Rating == Rating.S ? contentWarning : string.Empty),
                    Url = post.Page,
                    ImageUrl = post.Url,
                    Footer = new EmbedFooter
                    {
                        Text = (post.SourceUrl.Count() > 0) ? $"Source: {post.SourceUrl}" : $"{DateTime.Now.ToString("F")}",
                        IconUrl = "https://i.imgur.com/mrnHP3J.png"
                    },
                    Color = new DiscordColor(channel.EmbedColorHex)
                };

                // send the embed to the channel then remove the post from list, Update the next send time and exit the loop
                await discordChannel.SendMessageAsync("", false, eb);
                channel.RemovePost(post);
                channel.UpdateNextPostTime();
                i = false;
            }
        }

        /// <summary>
        /// Returns a List of posts.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        private async Task<List<Post>> PopulateChannelPostsAsync(PostChannel channel)
        {
            switch (channel.Rating)
            {
                case Rating.Q:
                    return await PullPosts(ConstantVars.QUES_SEARCH_QUERY_WAIFU);
                case Rating.E:
                    return await PullPosts(ConstantVars.NSFW_SEARCH_QUERY_WAIFU);
                case Rating.A:
                    return await PullPosts(ConstantVars.ANY_SEARCH_QUERY_WAIFU);
                default:
                    return await PullPosts(ConstantVars.SAFE_SEARCH_QUERY_WAIFU);
            }
        }

        /// <summary>
        /// Loads Saved Channel Data from file.
        /// </summary>
        /// <returns></returns>
        public async Task LoadChannelData()
        {
            var json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo(ChannelDataFilePath);
            if (!fi.Exists)
            {
                Channels = new List<PostChannel>();
                return;
            }

            using (var fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8))
                json = await sr.ReadToEndAsync();

            Channels = JsonConvert.DeserializeObject<List<PostChannel>>(json);
            return;
        }

        /// <summary>
        /// Saves Channel Data to file.
        /// </summary>
        /// <returns></returns>
        public async Task SaveChannelData()
        {
            var json = JsonConvert.SerializeObject(Channels, Formatting.Indented);
            File.WriteAllText(ChannelDataFilePath, json);
            return;
        }

        public PostChannel GetChannelByID(ulong channelid)
        {
            return Channels.First(c => c.ChannelId == channelid);
        }

        /// <summary>
        /// Pulls a list of posts from the CureNinja API.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<List<Post>> PullPosts(string query)
        {
            var jstring = await HttpRequestHandler.GetJsonStringAsync(query);

            var jobject = JObject.Parse(jstring);

            return jobject["results"].ToObject<List<Post>>();
        }

        public ImagePoster(DiscordClient discordClient)
        {
            _client = discordClient;
            doPost = true;
        }
    }
}