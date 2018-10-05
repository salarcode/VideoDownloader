using System;
using Newtonsoft.Json;

namespace VideoDownloader.App.Model
{
    public class Module
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("moduleId")]
        public Guid ModuleId { get; set; }

        [JsonProperty("deprecatedId")]
        public string DeprecatedId { get; set; }

        [JsonProperty("authorId")]
        public Guid AuthorId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("playerUrl")]
        public string PlayerUrl { get; set; }

        [JsonProperty("clips")]
        public Clip[] Clips { get; set; }
    }
}
