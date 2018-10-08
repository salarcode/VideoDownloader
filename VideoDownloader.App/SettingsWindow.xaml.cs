using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using VideoDownloader.App.ViewModel;

namespace VideoDownloader.App
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Messenger.Default.Register<NotificationMessage>(this, (message) =>
            {
                switch (message.Notification)
                {
                    case "CloseSettingsWindow":
                        Close();
                        break;
                }
            });
        }
    }
}

// implement System.Windows.Forms.IWin32Window