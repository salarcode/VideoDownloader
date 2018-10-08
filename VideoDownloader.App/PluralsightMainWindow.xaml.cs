using System.Windows;
using VideoDownloader.App.BL.Messages;
using VideoDownloader.App.ViewModel;

namespace VideoDownloader.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = (PluralsightMainViewModel)DataContext;
            Loaded += (sender, args) =>
            {
                vm.Messenger.Register<OpenSettingsMessage>(this, OpenSettingsWindow);
                vm.Messenger.Register<ExceptionThrownMessage>(this, ShowExceptionThrownMessageBox);
            };

            Unloaded += (sender, args) =>
            {
                vm.Messenger.Register<OpenSettingsMessage>(this, message => { });
                vm.Messenger.Register<ExceptionThrownMessage>(this, message => { });
            };
        }

        private void ShowExceptionThrownMessageBox(ExceptionThrownMessage message)
        {
            MessageBox.Show(message.Text, "Video downloader", MessageBoxButton.OK);
        }

        private void OpenSettingsWindow(OpenSettingsMessage message)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }
    }
}
