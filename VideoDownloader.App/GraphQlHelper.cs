using VideoDownloader.App.Model;
using VideoDownloader.App.Properties;

namespace VideoDownloader.App
{
    public static class GraphQlHelper
    {
        public static string GetClipsRequest(CourseRpcData courseRpcData, string author, string moduleId, int clipIndex)
        {
            var course = courseRpcData.Data.Rpc.BootstrapPlayer.Course;
            return "{\"query\": \"query viewClip {viewClip (input: {author: \\\"" 
                   + author + "\\\", " 
                   + $"clipIndex: {clipIndex}, "
                   + "courseName: \\\"" + course.Name + "\\\", includeCaptions: "
                   + course.CourseHasCaptions.ToString().ToLowerInvariant()
                   + ", locale: \\\"en\\\", mediaType: \\\"mp4\\\", moduleName: \\\""
                   + moduleId
                   + "\\\", quality: \\\""
                   + (course.SupportsWideScreenVideoFormats ? Settings.Default.Resolution1280x720 : Settings.Default.Resolution1024x768)
                   + "\\\"}) {urls {url cdn rank source}, status}}\", \"variables\": {}}";
        }

        public static string GetCourseExtraInfoRequest(string courseId)
        {
            return "{\"query\": \"query BootstrapPlayer {rpc {bootstrapPlayer {extraInfo: course(courseId: \\\"" + courseId + "\\\") {courseHasCaptions supportsWideScreenVideoFormats }}}}\", \"variables\": {}}";
        }

        public static string GetModuleInfoRequest(string courseId)
        {
            string s = "{\"query\":\"query BootstrapPlayer {rpc {bootstrapPlayer {profile {firstName lastName email username userHandle authed isAuthed plan }course(courseId: \\\""+courseId+"\\\") {name title courseHasCaptions translationLanguages {code name}supportsWideScreenVideoFormats timestamp modules {name title duration formattedDuration author authorized clips {authorized clipId duration formattedDuration id index moduleIndex moduleTitle name title watched }}}}}} \",\"variables\":{}}";
            return s;
        }
    }
}
