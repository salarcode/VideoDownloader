using System;
using Newtonsoft.Json;

namespace VideoDownloader.App.Model
{
    public class CourseRpcData
    {
        [JsonProperty("data")]
        public RpcData Data { get; set; }
    }

    public class RpcData
    {
        [JsonProperty("rpc")]
        public RpcInner Rpc { get; set; }
    }

    public class RpcInner
    {
        [JsonProperty("bootstrapPlayer")]
        public RpcBootstrapPlayer BootstrapPlayer { get; set; }
    }

    public class RpcBootstrapPlayer
    {
        [JsonProperty("profile")]
        public RpcProfile Profile { get; set; }

        [JsonProperty("course")]
        public RpcCourse Course { get; set; }
    }

    public class RpcCourse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("courseHasCaptions")]
        public bool CourseHasCaptions { get; set; }

        [JsonProperty("translationLanguages")]
        public RpcTranslationLanguage[] TranslationLanguages { get; set; }

        [JsonProperty("supportsWideScreenVideoFormats")]
        public bool SupportsWideScreenVideoFormats { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("modules")]
        public RpcModule[] Modules { get; set; }
    }

    public class RpcModule
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("formattedDuration")]
        public string FormattedDuration { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("authorized")]
        public bool Authorized { get; set; }

        [JsonProperty("clips")]
        public RpcClip[] Clips { get; set; }
    }

    public class RpcClip
    {
        [JsonProperty("authorized")]
        public bool Authorized { get; set; }

        [JsonProperty("clipId")]
        public Guid ClipId { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("formattedDuration")]
        public string FormattedDuration { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("index")]
        public long Index { get; set; }

        [JsonProperty("moduleIndex")]
        public long ModuleIndex { get; set; }

        [JsonProperty("moduleTitle")]
        public string ModuleTitle { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("watched")]
        public bool Watched { get; set; }
    }

    public class RpcTranslationLanguage
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RpcProfile
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("userHandle")]
        public Guid UserHandle { get; set; }

        [JsonProperty("authed")]
        public bool Authed { get; set; }

        [JsonProperty("isAuthed")]
        public bool IsAuthed { get; set; }

        [JsonProperty("plan")]
        public string Plan { get; set; }
    }
}
