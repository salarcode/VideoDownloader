using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoDownloader.App.BL;
using VideoDownloader.App.Model;

namespace VideoDownloader.App.Contracts
{
    interface ISubtitleService
    {
        void Write(string fileName, IList<SrtRecord> subtitleRecords);
        Task<string> DownloadAsync(HttpHelper httpHelper, string clipId, CancellationToken token);
    }
}