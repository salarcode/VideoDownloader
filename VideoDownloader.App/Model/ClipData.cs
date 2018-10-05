using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VideoDownloader.App.Model
{

    public class ClipData
    {
        [JsonProperty("data")]
        public ClipInnerData Data { get; set; }
    }

    public class ClipInnerData
    {
        [JsonProperty("viewClip")]
        public ViewClip ViewClip { get; set; }
    }

    public class ViewClip
    {
        [JsonProperty("urls")]
        public Url[] Urls { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }
    }

    public class Url
    {
        [JsonProperty("url")]
        public Uri UrlUrl { get; set; }

        [JsonProperty("cdn")]
        public string Cdn { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("source")]
        public object Source { get; set; }
    }
}
