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

namespace Suguya
{
    public class SuguyaCore
    {
        private SettingsStore _settingsStore { get; set; }
        private DiscordClient _client { get; set; }
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

            // hook events
            this._client.Ready += this.OnReady;

            await ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task ConnectAsync()
        {
            await _client.ConnectAsync().ConfigureAwait(false);
        }

        private Task OnReady(ReadyEventArgs e)
        {
            _client.UpdateStatusAsync(new DiscordActivity("conjured waifus.", ActivityType.Watching));
            return Task.CompletedTask;
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
                Console.WriteLine($"New settings file generated at {fi.FullName}\nPlease edit 'settings.json' and then relaunch.");
                Console.ReadLine();
            }

            using (var fs = fi.OpenRead())
            using (var sr = new StreamReader(fs, utf8))
                json = await sr.ReadToEndAsync();

            return JsonConvert.DeserializeObject<SettingsStore>(json);
        }
    }
}
