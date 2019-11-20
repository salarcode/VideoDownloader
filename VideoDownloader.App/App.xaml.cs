using CefSharp;
using CefSharp.Wpf;
using System;
using System.Windows;

namespace VideoDownloader.App
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			//Perform dependency check to make sure all relevant resources are in our output directory.
			var settings = new CefSettings();
			//settings.EnableInternalPdfViewerOffScreen();
			// Disable GPU in WPF and Offscreen examples until #1634 has been resolved
			settings.CefCommandLineArgs.Add("disable-gpu", "1");
			settings.CachePath = "browser-cache";

			Cef.Initialize(settings, performDependencyCheck: true, (IApp)null);

			try
			{
				var loginView = new LoginWindow();
				loginView.Show();
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}
