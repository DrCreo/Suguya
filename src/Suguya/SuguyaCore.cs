using DSharpPlus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Suguya.Models;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Suguya
{
    public class SuguyaCore
    {
        private SettingsStore _settingsStore { get; set; }
        private DiscordClient _client { get; set; }

        private TimeSpan timeBetweenPosts = TimeSpan.FromMinutes(5);
        private bool doPosting = true;

        private List<string> filters = ConstantVars.FILTERS.Split(" ").ToList();

        public async Task StartAsync()
        {
            _settingsStore = await LoadSettingsAsync();
            var config = new DiscordConfiguration
            {
                AutoReconnect = true,

                LargeThreshold = 250,
                LogLevel = LogLevel.Debug,

                MessageCacheSize = 2048,


                Token = _settingsStore.Token,
                TokenType = TokenType.Bot
            };

            _client = new DiscordClient(config);

            this._client.Ready += this.OnReadyAsync;

            await ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task ConnectAsync()
        {
            await _client.ConnectAsync().ConfigureAwait(false);
        }

        private async Task OnReadyAsync(ReadyEventArgs e)
        {
            PostWaifu();
            PostWaifuNSFW();

            await _client.UpdateStatusAsync(new DiscordActivity("conjured waifus.", ActivityType.Watching));
        }

        private async Task PostWaifu()
        {
            // Yes I know this is bad I'll fix it later I'm just testing.
            while (doPosting)
            {
                var jstring = await HttpRequestHandler.GetJsonStringAsync(ConstantVars.SAFE_SEARCH_QUERY_WAIFU);

                var jobject = JObject.Parse(jstring);

                var posts = jobject["results"].ToObject<List<Post>>();

                var channel = await _client.GetChannelAsync(315059561017114627);

                var rng = new Random();
                while (posts.Count > 0)
                {
                    var post = posts[rng.Next(0, posts.Count - 1)];
                    var eb = new DiscordEmbedBuilder
                    {
                        Title = $"{post.Source} (Warning may have NSFW content.",
                        Url = post.Page,
                        ImageUrl = post.Url,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Source: {post.SourceUrl}",
                            IconUrl = "https://i.imgur.com/mrnHP3J.png"
                        },
                        Color = new DiscordColor("9e8cb9")
                    };
                    if (post.Extension.ToLower() != "gif" && post.Extension.ToLower() != "webm" && post.Source.ToLower() != "sankakucomplex")
                    {
                        await channel.SendMessageAsync("", false, eb);
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }
                    posts.Remove(post);
                    Console.WriteLine($"SAFE COUNT: {posts.Count}\n");
                }
            }
        }

        private async Task PostWaifuNSFW()
        {
            // yeah.
            while (doPosting)
            {
                var jstring = await HttpRequestHandler.GetJsonStringAsync(ConstantVars.NSFW_SEARCH_QUERY_WAIFU);

                var jobject = JObject.Parse(jstring);

                var posts = jobject["results"].ToObject<List<Post>>();

                var channel = await _client.GetChannelAsync(315463551882100736);

                var rng = new Random();
                while (posts.Count > 0)
                {
                    var post = posts[rng.Next(0, posts.Count - 1)];
                    var eb = new DiscordEmbedBuilder
                    {
                        Title = $"{post.Source} (Warning may have NSFW content.",
                        Url = post.Page,
                        ImageUrl = post.Url,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Source: {post.SourceUrl}",
                            IconUrl = "https://i.imgur.com/mrnHP3J.png"
                        },
                        Color = new DiscordColor("9e8cb9")
                    };

                    if (!post.Tags.Any(t => filters.Contains(t)) && post.Extension.ToLower() != "gif" && post.Extension.ToLower() != "webm")
                    {
                        await channel.SendMessageAsync("", false, eb);
                        await Task.Delay(TimeSpan.FromMinutes(5));
                    }
                    posts.Remove(post);
                    Console.WriteLine($"NSFW COUNT: {posts.Count}\n");
                }
            }
        }

        private async Task<SettingsStore> LoadSettingsAsync()
        {
            var json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo("settings.json");
            if (!fi.Exists)
            {
                Console.WriteLine("loading settings failed.");

                json = JsonConvert.SerializeObject(SettingsStore.Defualt, Formatting.Indented);
                using (var fs = fi.Create())
                using (var sw = new StreamWriter(fs, utf8))
                {
                    await sw.WriteAsync(json);
                    await sw.FlushAsync();
                }
                Console.WriteLine($"New settings file generated at '{fi.FullName}'\nPlease edit 'settings.json' and then relaunch.");
                Console.ReadLine();
            }

            using (var fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8))
                json = await sr.ReadToEndAsync();

            return JsonConvert.DeserializeObject<SettingsStore>(json);
        }
    }
}
