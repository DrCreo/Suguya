using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Suguya.Models
{
    public class Post
    {
        [JsonProperty("tags")]
        private string TagString { get; set; }

        [JsonIgnore]
        public List<string> Tags
        {
            get
            {
                return TagString.Split(' ').ToList();
            }
        }

        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("page")]
        public string Page { get; private set; }

        [JsonProperty("preview")]
        public string Preview { get; private set; }

        [JsonProperty("source")]
        public string Source { get; private set; }

        [JsonProperty("sourceURL")]
        private string SourceUrlPrivate { get; set; }

        public string SourceUrl
        {
            get
            {
                return SourceUrlPrivate is null ? "" : SourceUrlPrivate;
            }
        }

        [JsonProperty("extension")]
        public string Extension { get; private set; }

        [JsonProperty("rating")]
        private string RatingString { get; set; }

        [JsonIgnore]
        public Rating Rating
        {
            get
            {
                switch (RatingString)
                {
                    case "s":
                        return Rating.S;
                    case "q":
                        return Rating.Q;
                    case "e":
                        return Rating.E;
                    default:
                        return Rating.NA;
                }
            }
        }
    }
}
