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

        public static SettingsStore Defualt
        {
            get
            {
                return new SettingsStore
                {
                    Token = "<token>",
                    Prefix = ">"
                };
            }
        }
    }
}
