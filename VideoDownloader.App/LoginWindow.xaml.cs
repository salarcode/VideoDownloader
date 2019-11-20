using CefSharp;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using VideoDownloader.App.Contracts;
using VideoDownloader.App.ViewModel;

namespace VideoDownloader.App
{
	/// <summary>
	/// Interaction logic for LoginWindow.xaml
	/// </summary>
	public partial class LoginWindow : ICloseable
	{
		public LoginWindow()
		{
			InitializeComponent();
			chromeBrowser.Address = "https://app.pluralsight.com/id?redirectTo=https://app.pluralsight.com/id/account";
			chromeBrowser.LoadHandler = (ILoadHandler)this.DataContext;
 
			Closing += (s, e) => ViewModelLocator.Cleanup();
			MouseLeftButtonDown += delegate { DragMove(); };
			Messenger.Default.Register<NotificationMessage>(this, (message) =>
			{
				switch (message.Notification)
				{
					case "CloseWindow":
						var mainWindow = new MainWindow();
						mainWindow.Show();
						Close();
						break;
				}
			});
		}
	}

	class MyClass : ILoadHandler
	{
		public void OnFrameLoadEnd(IWebBrowser chromiumWebBrowser, FrameLoadEndEventArgs frameLoadEndArgs)
		{
		}

		public void OnFrameLoadStart(IWebBrowser chromiumWebBrowser, FrameLoadStartEventArgs frameLoadStartArgs)
		{

		}

		public void OnLoadError(IWebBrowser chromiumWebBrowser, LoadErrorEventArgs loadErrorArgs)
		{

		}

		public void OnLoadingStateChange(IWebBrowser browser, LoadingStateChangedEventArgs loadingStateChangedArgs)
		{
			if (browser?.Address?.IndexOf("app.pluralsight.com/id", StringComparison.InvariantCultureIgnoreCase) == -1)
				return;
			if (loadingStateChangedArgs?.IsLoading == false)
			{
				MessageBox.Show("Done");
			}
		}
	}
}
