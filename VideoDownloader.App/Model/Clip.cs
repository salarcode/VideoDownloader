using System;
using Newtonsoft.Json;

namespace VideoDownloader.App.Model
{
    public class Clip
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("clipId")]
        public Guid ClipId { get; set; }

        [JsonProperty("deprecatedId")]
        public string DeprecatedId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("moduleTitle")]
        public string ModuleTitle { get; set; }

        [JsonProperty("playerUrl")]
        public string PlayerUrl { get; set; }

        [JsonProperty("ordering")]
        public long Ordering { get; set; }
    }
}
