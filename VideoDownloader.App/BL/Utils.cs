using System.IO;
using System.Text.RegularExpressions;

namespace VideoDownloader.App.BL
{
    public class Utils
    {
        public static string GetValidPath(string path)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            path = r.Replace(path, string.Empty);
            path = path.Trim(' ');
            return path;
        }

        public static string GetShortenedFileName(string fileNameWithoutExtension)
        {
            int ix1 = fileNameWithoutExtension.LastIndexOf('\\');
            int ix2 = fileNameWithoutExtension.LastIndexOf('\\', ix1 - 1);

            return fileNameWithoutExtension.Substring(ix2 + 1);
        }
    }
}
