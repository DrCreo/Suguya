using Newtonsoft.Json;
using System.Collections.Generic;

namespace Suguya.Models
{
    public class SettingsStore
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }

        public static SettingsStore Defualt
        {
            get
            {
                return new SettingsStore
                {
                    Token = "<token>",
                    Prefix = ">",
                };
            }
        }
    }
}
