using System;

namespace VideoDownloader.App.Model
{
    public class CourseRpc
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public Module[] Modules { get; set; }
        public bool HasTranscript { get; set; }
        public bool SupportsWideScreenVideoFormats { get; set; }
        //public dynamic[] TranslationLanguages { get; set; }
        //public DateTime Timestamp { get; set; }
    }
}
