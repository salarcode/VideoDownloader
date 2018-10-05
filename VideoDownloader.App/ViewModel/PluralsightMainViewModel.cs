using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using VideoDownloader.App.BL.Messages;
using VideoDownloader.App.Contracts;
using VideoDownloader.App.Model;
using VideoDownloader.App.Properties;

namespace VideoDownloader.App.ViewModel
{
    public class PluralsightMainViewModel : ViewModelBase
    {
        #region Fields

        private CancellationTokenSource _cancellationToken;

        private Dictionary<string, int> _numberOfCoursesForTag;
        private List<CourseDescription> _allCourses;
        private IEnumerable<CourseDescription> _currentDisplayedCourses;
        private ObservableCollection<CourseDescription> _currentDisplayedFilteredCourses;
        private readonly IConfigProvider _configProvider;
        private readonly ICourseService _courseService;
        private readonly ICourseMetadataService _courseMetadataService;

        private bool _isDownloading;
        private bool _anyCourseSelected;
        private string _courseBeingDownloaded;
        private string _currentAction;
        private int _downloadingProgress;
        private int _currentTimeout;
        private bool _canDownload;
        private int _numberOfSelectedCourses;
        private string _tagsFilterText;
        private string _coursesFilterText;
        private string _title;
        private string _currentUserAgent;
        private bool _onlyForSelectedTag;

        #endregion

        #region Properties

        public IMessenger Messenger => MessengerInstance;

        public bool CanDownload
        {
            get => _canDownload;
            set
            {
                Set(() => CanDownload, ref _canDownload, value);
                DownloadCourseCommand.RaiseCanExecuteChanged();
                CancelDownloadsCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                Set(() => IsDownloading, ref _isDownloading, value);
                DownloadCourseCommand.RaiseCanExecuteChanged();
                CancelDownloadsCommand.RaiseCanExecuteChanged();
            }
        }

        public int CurrentTimeout
        {
            get => _currentTimeout;
            set
            {
                Set(() => CurrentTimeout, ref _currentTimeout, value);
            }
        }

        public string CourseBeingDownloaded
        {
            get => _courseBeingDownloaded;
            set
            {
                Set(() => CourseBeingDownloaded, ref _courseBeingDownloaded, value);
            }

        }

        public string CurrentAction
        {
            get => _currentAction;
            set
            {
                Set(() => CurrentAction, ref _currentAction, value);
            }

        }

        public int DownloadingProgress
        {
            get => _downloadingProgress;
            set
            {
                Set(() => DownloadingProgress, ref _downloadingProgress, value);
            }
        }

        public bool AnyCourseSelected
        {
            get => _anyCourseSelected;
            set
            {
                Set(() => AnyCourseSelected, ref _anyCourseSelected, value);
            }
        }

        public int NumberOfSelectedCourses
        {
            get => _numberOfSelectedCourses;
            set
            {
                Set(() => NumberOfSelectedCourses, ref _numberOfSelectedCourses, value);
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                Set(() => Title, ref _title, value);
            }
        }

        public string CurrentUserAgent
        {
            get => _currentUserAgent ?? Resources.Undefined;
            set
            {
                Set(() => CurrentUserAgent, ref _currentUserAgent, value);
            }
        }

        public string TagsFilterText
        {
            get => _tagsFilterText ?? string.Empty;
            set
            {
                if (Set(() => TagsFilterText, ref _tagsFilterText, value))
                {
                    RaisePropertyChanged(nameof(NumberOfCoursesForTag));
                }
            }
        }

        public bool OnlyForSelectedTag
        {
            get => _onlyForSelectedTag;
            set
            {
                Set(() => OnlyForSelectedTag, ref _onlyForSelectedTag, value);
                RaisePropertyChanged(nameof(CoursesFilterText));
            }
        }

        public string CoursesFilterText
        {
            get => _coursesFilterText ?? string.Empty;
            set
            {
                Set(() => CoursesFilterText, ref _coursesFilterText, value);
                if (OnlyForSelectedTag)
                {
                    CurrentDisplayedFilteredCourses =
                    new ObservableCollection<CourseDescription>(CurrentDisplayedCourses
                    .Where(course => course.Title.ToLower().Contains(CoursesFilterText.ToLower()))
                    .OrderByDescending(c => c.PublishedDate));
                }
                else if (AllCourses.Any())
                {
                    CurrentDisplayedFilteredCourses =
                        new ObservableCollection<CourseDescription>(AllCourses
                        .Where(course => course.Title.ToLower().Contains(CoursesFilterText.ToLower()))
                        .OrderByDescending(c => c.PublishedDate));
                }
            }
        }

        public List<CourseDescription> AllCourses
        {
            get => _allCourses;
            set
            {
                Set(() => AllCourses, ref _allCourses, value);
            }
        }

        public Dictionary<string, int> NumberOfCoursesForTag
        {
            get
            {
                return _numberOfCoursesForTag.Where(tool => tool.Key.ToLower().Contains(TagsFilterText.ToLower()))
                      .OrderBy(tool => tool.Key).ToDictionary(p => p.Key, p => p.Value);
            }
            set
            {
                Set(() => NumberOfCoursesForTag, ref _numberOfCoursesForTag, value);
            }
        }

        public IEnumerable<CourseDescription> CurrentDisplayedCourses
        {
            get => _currentDisplayedCourses;
            set
            {
                Set(() => CurrentDisplayedCourses, ref _currentDisplayedCourses, value);
            }
        }

        public ObservableCollection<CourseDescription> CurrentDisplayedFilteredCourses
        {
            get => _currentDisplayedFilteredCourses;
            set
            {
                Set(() => CurrentDisplayedFilteredCourses, ref _currentDisplayedFilteredCourses, value);
            }
        }

        #endregion

        #region Command

        public RelayCommand<string> CourseTagSelectedCommand { get; set; }

        public RelayCommand OpenDownloadsFolderCommand { get; }

        public RelayCommand OpenSettingsWindowCommand { get; }

        public RelayCommand<bool> ProductCheckBoxToggledCommand { get; }

        public RelayCommand DownloadCourseCommand { get; set; }

        public RelayCommand CancelDownloadsCommand { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public PluralsightMainViewModel(ILoginService loginService, ICourseService courseService, ICourseMetadataService courseMetadataService, IConfigProvider configProvider)
        {
            _configProvider = configProvider;
            CurrentUserAgent = _configProvider.UserAgent;
            _courseService = courseService;
            _courseMetadataService = courseMetadataService;
            NumberOfSelectedCourses = 0;
            AuthenticatedUser authenticatedUser = JsonConvert.DeserializeObject<AuthenticatedUser>(loginService.LoginResultJson);
            Title = $"{authenticatedUser.CurrentUser.FirstName} {authenticatedUser.CurrentUser.LastName} ({authenticatedUser.CurrentUser.Email})";

            CourseTagSelectedCommand = new RelayCommand<string>(OnCourseTagSelected);
            CancelDownloadsCommand = new RelayCommand(OnCancelDownloads, CanCancelDownload);
            OpenDownloadsFolderCommand = new RelayCommand(OnOpenDownloadsFolder);
            OpenSettingsWindowCommand = new RelayCommand(OnOpenSettingsWindow);
            ProductCheckBoxToggledCommand = new RelayCommand<bool>(OnProductCheckBoxToggledCommand);
            DownloadCourseCommand = new RelayCommand(OnDownloadCourseAsync, CanExecuteDownload);
            NumberOfCoursesForTag = _courseService.CoursesByToolName.ToDictionary(kvp => kvp.Key, v => v.Value.Count);
            AllCourses = _courseService.CoursesByToolName.Values.SelectMany(x => x).Distinct().ToList();

        }

        #endregion

        #region Command executors

        private void OnOpenDownloadsFolder()
        {
            Process.Start(_configProvider.DownloadsPath);
        }

        private void OnOpenSettingsWindow()
        {
            Messenger.Send(new OpenSettingsMessage());
        }

        private bool CanExecuteDownload()
        {
            return !IsDownloading && AnyCourseSelected;
        }

        private bool CanCancelDownload()
        {
            return IsDownloading;
        }

        private void OnProductCheckBoxToggledCommand(bool isChecked)
        {
            if (isChecked)
            {
                ++NumberOfSelectedCourses;
            }
            else
            {
                --NumberOfSelectedCourses;
            }
            AnyCourseSelected = (NumberOfSelectedCourses > 0);
            RaisePropertyChanged(() => AnyCourseSelected);
        }

        void OnDownloadingProgressChanged(object sender, CourseDownloadingProgressArguments e)
        {
            DownloadingProgress = e.ClipProgress;
            CurrentAction = e.CurrentAction;
            CourseBeingDownloaded = e.ClipName;
        }
        void OnTimeoutProgressChanged(object sender, int e)
        {
            CurrentTimeout = e;
        }

        private async void OnDownloadCourseAsync()
        {
            var downloadingProgress = new Progress<CourseDownloadingProgressArguments>();
            var timeoutProgress = new Progress<int>();

            try
            {
                IsDownloading = true;
                _cancellationToken = new CancellationTokenSource();

                downloadingProgress.ProgressChanged += OnDownloadingProgressChanged;

                timeoutProgress.ProgressChanged += OnTimeoutProgressChanged;

                var coursesToDownload = CurrentDisplayedFilteredCourses.Where(c => c.CheckedForDownloading);
                foreach (var courseDescription in coursesToDownload)
                {
                    string fullInformation = await _courseService.GetFullCourseInformationAsync(courseDescription.Id, _cancellationToken.Token);
                    Course courseFullInfo = JsonConvert.DeserializeObject<Course>(fullInformation);

                    var tableOfContent = _courseService.CreateTableOfContent(fullInformation);
                    string destinationFolder = _configProvider.DownloadsPath;
                    string validBaseCourseDirectory = $"{_courseService.GetBaseCourseDirectoryName(destinationFolder, courseFullInfo.Title)}";

                    _courseMetadataService.WriteTableOfContent(validBaseCourseDirectory, tableOfContent);
                    _courseMetadataService.WriteDescription(validBaseCourseDirectory, courseFullInfo.Description);
                    await _courseService.DownloadAsync(courseFullInfo, downloadingProgress, timeoutProgress, _cancellationToken.Token);

                    courseDescription.CheckedForDownloading = false;
                    --NumberOfSelectedCourses;
                }
                IsDownloading = false;
            }
            catch (OperationCanceledException)
            {
                Messenger.Send(new ExceptionThrownMessage(Resources.YouCanceledDownloading));
            }
            catch (JsonSerializationException exc)
            {
                Messenger.Send(new ExceptionThrownMessage(exc.Message));
            }
            catch (JsonException exc)
            {
                Messenger.Send(new ExceptionThrownMessage(exc.Message));
            }
            catch (Exception exc)
            {
                Messenger.Send(new ExceptionThrownMessage(exc.Message));
            }
            finally
            {
                downloadingProgress.ProgressChanged -= OnDownloadingProgressChanged;
                timeoutProgress.ProgressChanged -= OnTimeoutProgressChanged;
            }
        }

        private async void OnCourseTagSelected(string toolName)
        {
            CurrentDisplayedCourses = await _courseService.GetToolCourses(toolName);
            if (CurrentDisplayedCourses.Any())
            {
                CurrentDisplayedFilteredCourses =
                    new ObservableCollection<CourseDescription>(CurrentDisplayedCourses
                    .Where(course => course.Title.ToLower().Contains(CoursesFilterText.ToLower()))
                    .OrderByDescending(c => c.PublishedDate));
            }

            AnyCourseSelected = (NumberOfSelectedCourses > 0);
        }

        void OnCancelDownloads()
        {
            _cancellationToken?.Cancel();
            IsDownloading = false;
        }

        #endregion
    }
}