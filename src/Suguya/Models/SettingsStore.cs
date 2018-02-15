using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Suguya.Models
{
    public class SettingsStore
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }

        [JsonProperty("waifu-channel-ids")]
        public List<ulong> WaifuChannelIds { get; private set; }

        [JsonProperty("waifu-channel-nsfw-ids")]
        public List<ulong> WaifuChannelNsfwIds { get; private set; }

        [JsonProperty("filters")]
        public List<string> Filters { get; private set; }

        public static SettingsStore Defualt
        {
            get
            {
                return new SettingsStore
                {
                    Token = "<token>",
                    Prefix = ">",
                    WaifuChannelIds = new List<ulong>(),
                    WaifuChannelNsfwIds = new List<ulong>(),
                    Filters = new List<string>();
                };
            }
        }
    }
}
