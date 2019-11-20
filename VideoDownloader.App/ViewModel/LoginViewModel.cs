using CefSharp;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VideoDownloader.App.Contracts;

namespace VideoDownloader.App.ViewModel
{
	public class LoginViewModel : ViewModelBase, IDataErrorInfo, ILoadHandler
	{
		#region Fields

		private readonly ICourseService _courseService;
		private readonly ILoginService _loginService;
		private string _userName;

		private bool _loginButtonEnabled;
		private bool _loggingInAnimationVisible;
		private bool _firstUserNameCheck = true;
		private string _currentOperation = string.Empty;
		private bool _useCachedListOfProducts;
		private string _lastUrl;

		#endregion

		#region Constructors

		public LoginViewModel(ILoginService loginService, ICourseService сourseService)
		{
			_courseService = сourseService;
			_loginService = loginService;
			LoginButtonEnabled = true;
			LoginInProgress = false;
			LoginCommand = new RelayCommand<object>(obj => OnLogin(obj), Login_CanExecute);
			CloseCommand = new RelayCommand<ICloseable>(CloseWindow);
		}
		#endregion

		#region Properties

		public RelayCommand<object> LoginCommand { get; }

		public RelayCommand<ICloseable> CloseCommand { get; }

		public bool UseCachedListOfProducts
		{
			get => _useCachedListOfProducts;
			set { Set(() => UseCachedListOfProducts, ref _useCachedListOfProducts, value); }
		}

		public string UserName
		{
			get => _userName;

			set
			{
				Set(() => UserName, ref _userName, value);
				LoginButtonEnabled = ValidateUserName() && !LoginInProgress;
			}
		}

		private string Password { get; set; }

		public string CurrentOperation
		{
			get => _currentOperation;
			set
			{
				Set(() => CurrentOperation, ref _currentOperation, value);
			}
		}

		public bool LoginButtonEnabled
		{
			get => _loginButtonEnabled;
			set
			{
				Set(() => LoginButtonEnabled, ref _loginButtonEnabled, value);
			}
		}

		public bool ShowAnimation
		{
			get => _loggingInAnimationVisible;
			set
			{
				Set(() => ShowAnimation, ref _loggingInAnimationVisible, value);
			}
		}

		private bool LoginInProgress { get; set; }

		#endregion

		#region commands

		#endregion

		#region command executors

		private async Task OnLogin(object passwordControl)
		{
			LoginButtonEnabled = false;
			Password = GetPassword(passwordControl);
			CurrentOperation = Properties.Resources.TryingToLogin;
			LoginInProgress = true;

			LoginResult loginResult = await _loginService.LoginAsync(UserName, Password);

			if (loginResult.Status == LoginStatus.LoggedIn)
			{
				await ProcessSuccessfulLogin(loginResult.Cookies);
			}
			else
			{
				CurrentOperation = Properties.Resources.LoginFailed;
				LoginInProgress = false;
				LoginButtonEnabled = true;
			}
		}

		internal async Task ProcessSuccessfulLogin(string cookies)
		{
			CurrentOperation = UseCachedListOfProducts ? Properties.Resources.ReadingCachedProducts : Properties.Resources.DownloadingListOfProducts;
			_courseService.Cookies = cookies;

			bool received = UseCachedListOfProducts ? await _courseService.ProcessCachedProductsAsync() : await _courseService.ProcessNoncachedProductsJsonAsync();
			if (received)
			{
				File.WriteAllText(Properties.Settings.Default.FileNameForJsonOfCourses, _courseService.CachedProductsJson);
				Messenger.Default.Send(new NotificationMessage("CloseWindow"));
			}
			else
			{
				CurrentOperation = Properties.Resources.UnableToReceiveProducts;
				LoginInProgress = false;
				LoginButtonEnabled = true;
			}
		}

		private static string GetPassword(object passwordControl)
		{
			System.Windows.Controls.PasswordBox pwBoxControl = passwordControl as System.Windows.Controls.PasswordBox;
			Debug.Assert(pwBoxControl != null, "pwBoxControl != null");
			var password = pwBoxControl.Password;
			return password;
		}

		private void CloseWindow(ICloseable window)
		{
			window?.Close();
		}

		private bool Login_CanExecute(object o)
		{
			return LoginButtonEnabled && ValidateUserName();
		}

		#endregion

		#region Auxiliary methods

		private bool ValidateUserName() => !string.IsNullOrEmpty(UserName);

		#endregion

		#region IDataError members

		public string this[string columnName]
		{
			get
			{
				string msg = null;
				switch (columnName)
				{
					case "UserName":
						{
							if (!_firstUserNameCheck && !ValidateUserName())
								msg = Properties.Resources.NameCannotBeEmpty;
							else
							{
								_firstUserNameCheck = false;
							}
							break;
						}
				}
				return msg;
			}
		}

		public string Error => string.Empty;

		#endregion

		#region Browser ILoadHandler

		string GetBrowserAddress(IWebBrowser browser)
		{
			try
			{
				return browser?.Address;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void OnLoadingStateChange(IWebBrowser browser, LoadingStateChangedEventArgs loadingStateChangedArgs)
		{
			var address = GetBrowserAddress(browser) ?? _lastUrl ?? "";
			if (address.IndexOf("https://app.pluralsight.com/id", StringComparison.InvariantCultureIgnoreCase) != 0)
				return;
			if (loadingStateChangedArgs.IsLoading == false)
			{
				var manager = Cef.GetGlobalCookieManager();
				manager.VisitUrlCookies(address, true, new LoginCookieReader(this));
			}
		}

		public void OnFrameLoadStart(IWebBrowser browser, FrameLoadStartEventArgs frameLoadStartArgs)
		{

		}

		public void OnFrameLoadEnd(IWebBrowser browser, FrameLoadEndEventArgs frameLoadEndArgs)
		{
			_lastUrl = frameLoadEndArgs.Url;

		}

		public void OnLoadError(IWebBrowser browser, LoadErrorEventArgs loadErrorArgs)
		{

		}
		#endregion

	}

	internal class LoginCookieReader : ICookieVisitor
	{
		private LoginViewModel loginViewModel;
		private readonly CookieContainer _cookies;

		public LoginCookieReader(LoginViewModel loginViewModel)
		{
			this.loginViewModel = loginViewModel;
			_cookies = new CookieContainer();
		}

		public void Dispose()
		{
		}

		public bool Visit(CefSharp.Cookie cookie, int count, int total, ref bool deleteCookie)
		{
			if (!string.IsNullOrWhiteSpace(cookie.Value))
			{
				try
				{
					var c = new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
					_cookies.Add(new Uri("https://app.pluralsight.com/id"), c);
				}
				catch (Exception)
				{
					// ignore names not recognised by .net
				}
			}

			if (count == total - 1)
			{
				var cookieHeader = _cookies.GetCookieHeader(new Uri("https://app.pluralsight.com/id"));
				loginViewModel.ProcessSuccessfulLogin(cookieHeader).Wait();
			}
			return true;
		}
	}
}
