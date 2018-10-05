using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VideoDownloader.App.Contracts;
using VideoDownloader.App.Model;

namespace VideoDownloader.App.BL
{
    class SubtitleService: ISubtitleService
    {
        public async Task<string> DownloadAsync(HttpHelper httpHelper, string clipId, CancellationToken token)
        {
            string url = $"https://app.pluralsight.com/transcript/api/v1/caption/json/{clipId}/en";
            ResponseEx response = await httpHelper.SendRequest(HttpMethod.Get,
                             new Uri(url),
                             null,
                             Properties.Settings.Default.RetryOnRequestFailureCount, token);
            return response.Content;
        }

        public void Write(string fileName, IList<SrtRecord> subtitleRecords)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                int index = 1;
                foreach (var record in subtitleRecords)
                {
                    sw.WriteLine(index);
                    sw.WriteLine($"{record.FromTimeSpan:hh':'mm':'ss'.'fff} --> {record.ToTimeSpan:hh':'mm':'ss'.'fff}");
                    sw.WriteLine(record.Text);
                    sw.WriteLine();
                    ++index;
                }
            }
        }

        private string BuildSubtitlePostDataJson(string authorId, int partNumber, string moduleName)
        {
            SubtitlePostData viewclipData = new SubtitlePostData()
            {
                Author = authorId,
                ClipIndex = partNumber,
                Locale = Properties.Settings.Default.EnglishLocale,
                ModuleName = moduleName
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(viewclipData);
        }
    }
}
