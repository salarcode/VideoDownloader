using Newtonsoft.Json;

namespace VideoDownloader.App.Model
{
    public class CourseExtraInfo
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("rpc")]
        public Rpc Rpc { get; set; }
    }

    public class Rpc
    {
        [JsonProperty("bootstrapPlayer")]
        public BootstrapPlayer BootstrapPlayer { get; set; }
    }

    public class BootstrapPlayer
    {
        [JsonProperty("extraInfo")]
        public ExtraInfo ExtraInfo { get; set; }
    }

    public class ExtraInfo
    {
        [JsonProperty("courseHasCaptions")]
        public bool CourseHasCaptions { get; set; }

        [JsonProperty("supportsWideScreenVideoFormats")]
        public bool SupportsWideScreenVideoFormats { get; set; }
    }
}
