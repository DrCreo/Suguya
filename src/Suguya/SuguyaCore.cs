using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using Suguya.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DSharpPlus.Interactivity;

namespace Suguya
{
    public class SuguyaCore
    {
        private SettingsStore _settingsStore { get; set; }
        private DiscordClient _client { get; set; }
        private ImagePoster _imagePoster { get; set; }
        private CommandsNextExtension _commandsNext { get; set; }
        private InteractivityExtension _interactivity { get; set; }

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

            _imagePoster = new ImagePoster(_client);

            var deps = new ServiceCollection()
                .AddSingleton(_imagePoster)
                .AddSingleton(_client);

            var prefixes = new List<string>();
            prefixes.Add(_settingsStore.Prefix);

            var cnConfig = new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDefaultHelp = true,
                EnableDms = false,
                Services = deps.BuildServiceProvider(),
                EnableMentionPrefix = true,
                StringPrefixes = prefixes
            };
            _commandsNext = _client.UseCommandsNext(cnConfig);

            // register commands
            _commandsNext.RegisterCommands(Assembly.GetExecutingAssembly());

            _interactivity = _client.UseInteractivity(new InteractivityConfiguration());

            _client.Ready += this.OnReadyAsync;

            await ConnectAsync();
            await Task.Delay(-1);
        }

        private async Task ConnectAsync()
        {
            await _client.ConnectAsync().ConfigureAwait(false);
        }

        private async Task OnReadyAsync(ReadyEventArgs e)
        {
            _imagePoster.StartPosting();
            await _client.UpdateStatusAsync(new DiscordActivity("conjured waifus.", ActivityType.Watching));
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
