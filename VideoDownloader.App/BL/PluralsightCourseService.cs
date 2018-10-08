using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Newtonsoft.Json;
using VideoDownloader.App.BL.Exceptions;
using VideoDownloader.App.Contracts;
using VideoDownloader.App.Model;
using VideoDownloader.App.Properties;
using Timer = System.Timers.Timer;

namespace VideoDownloader.App.BL
{
    class PluralsightCourseService : ICourseService, IDisposable
    {
        #region Fields

        private readonly object _syncObj = new object();
        private readonly Timer _timeoutBetweenClipDownloadingTimer = new Timer(1000);

        private readonly IConfigProvider _configProvider;
        private readonly ISubtitleService _subtitleService;

        private readonly string _userAgent;
        private CancellationToken _token;
        private IProgress<CourseDownloadingProgressArguments> _courseDownloadingProgress;

        private int _totalCourseDownloadingProgessRatio;
        private int _timeout;
        private IProgress<int> _timeoutProgress;
        private bool _disposed;

        #endregion

        #region Constructors

        public PluralsightCourseService(IConfigProvider configProvider, ISubtitleService subtitleService)
        {
            _configProvider = configProvider;
            _subtitleService = subtitleService;
            _userAgent = _configProvider.UserAgent;
        }

        #endregion

        #region Properties

        public string Cookies { get; set; }

        public Dictionary<string, List<CourseDescription>> CoursesByToolName { get; set; } = new Dictionary<string, List<CourseDescription>>();

        public string CachedProductsJson { get; private set; }

        #endregion

        private int GenerateRandomNumber(int min, int max)
        {
            lock (_syncObj)
            {
                var random = new Random();
                return random.Next(min, max);
            }
        }

        public async Task DownloadAsync(Course course,
          IProgress<CourseDownloadingProgressArguments> downloadingProgress,
          IProgress<int> timeoutProgress,
          CancellationToken token)
        {
            _timeoutProgress = timeoutProgress;
            _courseDownloadingProgress = downloadingProgress;
            _token = token;
            var courseExtraInfo = await GetCourseExtraInfo(course.Id, _token);
            course.SupportsWideScreen = courseExtraInfo.Data.Rpc.BootstrapPlayer.ExtraInfo.SupportsWideScreenVideoFormats;
            await DownloadCourse(course);
        }

        public string CreateTableOfContent(string courseJson)
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(courseJson);
            int moduleCounter = 0;
            StringBuilder tableOfContent = new StringBuilder();

            foreach (var module in json.modules)
            {
                int clipCounter = 0;
                ++moduleCounter;
                TimeSpan moduleTimeSpan = XmlConvert.ToTimeSpan((string)module.duration);
                string moduleDuration = moduleTimeSpan.ToString(@"hh\:mm\:ss");
                tableOfContent.AppendLine($"{moduleCounter,3}.{module.title} {moduleDuration}");
                foreach (var clip in module.clips)
                {
                    ++clipCounter;
                    TimeSpan clipTimeSpan = XmlConvert.ToTimeSpan((string)clip.duration);
                    string clipDuration = clipTimeSpan.ToString(@"hh\:mm\:ss");
                    tableOfContent.AppendLine($"\t{clipCounter,3}.{clip.title} {clipDuration}");
                }
            }

            return tableOfContent.ToString();
        }

        public async Task<string> GetFullCourseInformationAsync(string productId, CancellationToken token)
        {
            string url = $"https://app.pluralsight.com/learner/content/courses/{productId}";

            var httpHelper = new HttpHelper
            {
                AcceptHeader = AcceptHeader.JsonTextPlain,
                AcceptEncoding = string.Empty,
                ContentType = ContentType.AppJsonUtf8,
                Cookies = Cookies,
                Referrer = new Uri($"https://{Settings.Default.SiteHostName}"),
                UserAgent = _userAgent
            };
            var courseRespone = await httpHelper.SendRequest(HttpMethod.Get, new Uri(url), null, Settings.Default.RetryOnRequestFailureCount, token);

            return courseRespone.Content;
        }

        private async Task DownloadCourse(Course course)
        {
            string destinationFolder = _configProvider.DownloadsPath;

            _timeoutBetweenClipDownloadingTimer.Elapsed += OnTimerElapsed;

            var courseDirectory = CreateCourseDirectory(GetBaseCourseDirectoryName(destinationFolder, course.Title));
            try
            {
                var moduleCounter = 0;
                foreach (var module in course.Modules)
                {
                    ++moduleCounter;
                    await DownloadModule(course, courseDirectory, moduleCounter, module);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                var progressArgs = new CourseDownloadingProgressArguments
                {
                    ClipName = string.Empty,
                    CourseProgress = 0,
                    ClipProgress = 0
                };
                _timeoutBetweenClipDownloadingTimer.Elapsed -= OnTimerElapsed;
                _courseDownloadingProgress.Report(progressArgs);
                if (_timeoutBetweenClipDownloadingTimer != null)
                {
                    _timeoutBetweenClipDownloadingTimer.Enabled = false;
                }
                _timeoutProgress.Report(0);
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_timeout > 0)
            {
                _timeoutProgress.Report(--_timeout);
            }
        }

        private async Task DownloadModule(Course course,
            string courseDirectory,
            int moduleCounter, Module module)
        {
            var moduleDirectory = CreateModuleDirectory(courseDirectory, moduleCounter, module.Title);
            var clipCounter = 0;
            string referrer = $"https://{Settings.Default.SiteHostName}/player?course={course.Id}&author={module.AuthorId}&name={module.ModuleId}&clip=0&mode=live";

            HttpHelper httpHelper = new HttpHelper
            {
                AcceptEncoding = string.Empty,
                AcceptHeader = AcceptHeader.All,
                ContentType = ContentType.AppJsonUtf8,
                Cookies = Cookies,
                Referrer = new Uri(referrer),
                UserAgent = _userAgent
            };

            string moduleInfoPayload = GraphQlHelper.GetModuleInfoRequest(course.Id);
            ResponseEx courseRpcResponse = await httpHelper.SendRequest(HttpMethod.Post, new Uri("https://" + Settings.Default.SiteHostName + "/player/api/graphql"), moduleInfoPayload, Settings.Default.RetryOnRequestFailureCount, _token);
            CourseRpcData courceRpcData = JsonConvert.DeserializeObject<CourseRpcData>(courseRpcResponse.Content);
            foreach (var clip in module.Clips)
            {
                ++clipCounter;

                var fileName = GetFullFileNameWithoutExtension(clipCounter, moduleDirectory, clip);

                if (!File.Exists($"{fileName}.{Settings.Default.ClipExtensionMp4}"))
                {
                    httpHelper.Referrer = new Uri($"https://{Settings.Default.SiteHostName}/player?course={course.Id}&author={module.AuthorId}&name={module.ModuleId}&clip=0&mode=live");
                    string s = GraphQlHelper.GetClipsRequest(courceRpcData, course.Authors[0].Id, module.Id.Split('|')[2], clipCounter - 1);
                    ResponseEx viewclipResponse = await httpHelper.SendRequest(HttpMethod.Post, new Uri("https://" + Settings.Default.SiteHostName + "/player/api/graphql"), s, Settings.Default.RetryOnRequestFailureCount, _token);
                    if (viewclipResponse.Content == "Unauthorized")
                    {
                        throw new UnauthorizedException(Resources.CheckYourSubscription);
                    }

                    var clipData = JsonConvert.DeserializeObject<ClipData>(viewclipResponse.Content);

                    if (course.HasTranscript)
                    {

                        string unformattedSubtitlesJson =
                            await _subtitleService.DownloadAsync(httpHelper, clip.ClipId.ToString(), _token);
                        Caption[] unformattedSubtitles =
                            JsonConvert.DeserializeObject<Caption[]>(unformattedSubtitlesJson);
                        if (unformattedSubtitles.Length > 0)
                        {
                            TimeSpan totalDurationTs = XmlConvert.ToTimeSpan(clip.Duration);

                            IList<SrtRecord> formattedSubtitles =
                                GetFormattedSubtitles(unformattedSubtitles, totalDurationTs);
                            _subtitleService.Write($"{fileName}.{Settings.Default.SubtitilesExtensionMp4}",
                                formattedSubtitles);
                        }
                    }

                    await DownloadClip(clipData.Data.ViewClip.Urls[1].UrlUrl,
                      fileName,
                      clipCounter,
                      course.Modules.Sum(m => m.Clips.Length));
                }
            }
        }

        private List<SrtRecord> GetFormattedSubtitles(Caption[] captions, TimeSpan totalDuration)
        {
            List<SrtRecord> srtRecords = new List<SrtRecord>();
            CultureInfo culture = new CultureInfo("en-US");

            for (int i = 0; i < captions.Length - 1; ++i)
            {
                SrtRecord srtRecord = new SrtRecord
                {
                    FromTimeSpan = TimeSpan.FromSeconds(double.Parse(captions[i].DisplayTimeOffset, culture)),
                    ToTimeSpan = TimeSpan.FromSeconds(double.Parse(captions[i + 1].DisplayTimeOffset, culture)),
                    Text = (captions[i].Text)
                };
                srtRecords.Add(srtRecord);
            }

            SrtRecord finalSrtRecord = new SrtRecord
            {
                FromTimeSpan = TimeSpan.FromSeconds(Double.Parse(captions.Last().DisplayTimeOffset, culture)),
                ToTimeSpan = totalDuration,
                Text = captions.Last().Text
            };

            srtRecords.Add(finalSrtRecord);
            return srtRecords;
        }

        private async Task DownloadClip(Uri clipUrl, string fileNameWithoutExtension, int clipCounter, int partsNumber)
        {
            _token.ThrowIfCancellationRequested();
            Progress<FileDownloadingProgressArguments> fileDownloadingProgress = null;
            try
            {
                RemovePartiallyDownloadedFile(fileNameWithoutExtension);

                var httpHelper = new HttpHelper
                {
                    AcceptEncoding = string.Empty,
                    AcceptHeader = AcceptHeader.All,
                    ContentType = ContentType.Video,
                    Cookies = Cookies,
                    Referrer = new Uri(Settings.Default.ReferrerUrlForDownloading),
                    UserAgent = _userAgent
                };

                string fileNameForProgressReport = Utils.GetShortenedFileName(fileNameWithoutExtension);

                _totalCourseDownloadingProgessRatio = (int)(((double)clipCounter) / partsNumber * 100);
                _courseDownloadingProgress.Report(new CourseDownloadingProgressArguments
                {
                    CurrentAction = Resources.Downloading,
                    ClipName = $"{fileNameForProgressReport}.{Settings.Default.ClipExtensionPart}",
                    CourseProgress = _totalCourseDownloadingProgessRatio,
                    ClipProgress = 0
                });

                fileDownloadingProgress = new Progress<FileDownloadingProgressArguments>();
                fileDownloadingProgress.ProgressChanged += OnProgressChanged;

                await httpHelper.DownloadWithProgressAsync(clipUrl,
                    $"{fileNameWithoutExtension}.{Settings.Default.ClipExtensionMp4}",
                    fileDownloadingProgress,
                    Settings.Default.RetryOnRequestFailureCount, _token);

                _courseDownloadingProgress.Report(new CourseDownloadingProgressArguments
                {
                    CurrentAction = Resources.Downloaded,
                    ClipName = $"{fileNameForProgressReport}.{Settings.Default.ClipExtensionMp4}",
                    CourseProgress = _totalCourseDownloadingProgessRatio,
                    ClipProgress = 0
                });

                _timeoutBetweenClipDownloadingTimer.Enabled = true;
                _timeout = GenerateRandomNumber(_configProvider.MinTimeout, _configProvider.MaxTimeout);
                await Task.Delay(_timeout * 1000, _token);
                _timeoutBetweenClipDownloadingTimer.Enabled = false;
            }

            finally
            {
                _timeoutProgress.Report(0);
                if (fileDownloadingProgress != null)
                {
                    fileDownloadingProgress.ProgressChanged -= OnProgressChanged;
                }
            }
        }

        private void OnProgressChanged(object sender, FileDownloadingProgressArguments e)
        {
            var progressArgs = new CourseDownloadingProgressArguments
            {
                CurrentAction = Resources.Downloading,
                ClipName = e.FileName,
                CourseProgress = _totalCourseDownloadingProgessRatio,
                ClipProgress = e.Percentage
            };
            _courseDownloadingProgress.Report(progressArgs);
        }


        private static void RemovePartiallyDownloadedFile(string fileNameWithoutExtension)
        {
            File.Delete($"{fileNameWithoutExtension}.{Settings.Default.ClipExtensionPart}");
        }

        private string GetFullFileNameWithoutExtension(int clipCounter, string moduleDirectory, Clip clip)
        {
            return $@"{moduleDirectory}\{clipCounter:00}.{Utils.GetValidPath(clip.Title)}";
        }

        private string CreateModuleDirectory(string courseDirectory, int moduleCounter, string moduleTitle)
        {
            var moduleDirectory = $@"{courseDirectory}\{moduleCounter:00}.{Utils.GetValidPath(moduleTitle)}";
            return Directory.CreateDirectory(moduleDirectory).FullName;
        }

        private string CreateCourseDirectory(string destinationFolder)
        {
            return Directory.CreateDirectory(destinationFolder).FullName;
        }

        private async Task<CourseExtraInfo> GetCourseExtraInfo(string productId, CancellationToken token)
        {
            HttpHelper httpHelper = new HttpHelper
            {
                AcceptHeader = AcceptHeader.JsonTextPlain,
                AcceptEncoding = string.Empty,
                ContentType = ContentType.AppJsonUtf8,
                Cookies = Cookies,
                Referrer = new Uri("https://" + Settings.Default.SiteHostName),
                UserAgent = _userAgent
            };
            string graphQlRequest = GraphQlHelper.GetCourseExtraInfoRequest(productId);
            ResponseEx response = await httpHelper.SendRequest(HttpMethod.Post, new Uri("https://" + Settings.Default.SiteHostName + "/player/api/graphql"), graphQlRequest, Settings.Default.RetryOnRequestFailureCount, token);

            return JsonConvert.DeserializeObject<CourseExtraInfo>(response.Content);
        }

        public string GetBaseCourseDirectoryName(string destinationDirectory, string courseName)
        {
            return $"{destinationDirectory}\\Pluralsight - {Utils.GetValidPath(courseName)}";
        }

        public async Task<bool> ProcessNoncachedProductsJsonAsync()
        {
            try
            {
                CachedProductsJson = await DownloadProductsJsonAsync();
                ProcessResult();
                return !string.IsNullOrEmpty(CachedProductsJson);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> DownloadProductsJsonAsync()
        {
            var httpHelper = new HttpHelper
            {
                AcceptEncoding = string.Empty,
                AcceptHeader = AcceptHeader.HtmlXml,
                ContentType = ContentType.AppXWwwFormUrlencode,
                Cookies = Cookies,
                Referrer = new Uri($"https://{Settings.Default.SiteHostName}"),
                UserAgent = _userAgent
            };
            var productsJsonResponse = await httpHelper.SendRequest(HttpMethod.Get,
                new Uri(Settings.Default.AllCoursesUrl),
                null,
                Settings.Default.RetryOnRequestFailureCount, _token);

            return productsJsonResponse.Content;
        }


        private void ProcessResult()
        {
            CoursesByToolName.Clear();
            var allProducts = JsonConvert.DeserializeObject<AllProducts>(CachedProductsJson);
            foreach (var product in allProducts.ResultSets[0].Results)
            {
                try
                {
                    var tools = product.Tools?.Split('|') ?? new[] { "-" };
                    foreach (var tool in tools)
                    {
                        if (!CoursesByToolName.ContainsKey(tool))
                        {
                            CoursesByToolName[tool] = new List<CourseDescription>();
                        }
                        CoursesByToolName[tool].Add(product);
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        public async Task<bool> ProcessCachedProductsAsync()
        {
            try
            {
                if (File.Exists(Settings.Default.FileNameForJsonOfCourses))
                {
                    CachedProductsJson = await Task.Run(() => File.ReadAllText(Settings.Default.FileNameForJsonOfCourses), _token);
                    ProcessResult();
                }
                else
                {
                    await ProcessNoncachedProductsJsonAsync();
                }

                return !string.IsNullOrEmpty(CachedProductsJson);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<CourseDescription>> GetToolCourses(string toolName)
        {
            var courses = await Task.Run(() => CoursesByToolName.Single(kvp => kvp.Key == toolName).Value);
            return courses;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _timeoutBetweenClipDownloadingTimer?.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
