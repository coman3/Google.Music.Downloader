using System.Windows;
using System.Windows.Controls;
using GoogleMusicApi.Common;

namespace Google.Music.Downloader
{
    public static class ApplicationState
    {
        public static Page CurrentPage { get; set; }
        public static MainWindow MainWindow { get; set; }

        public static MobileClient MobileClient { get; private set; }

        static ApplicationState()
        {
            MobileClient = new MobileClient();
        }
        public static void SetWindow(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        public static void SetPage(Page page)
        {
            MainWindow.Content = CurrentPage = page;
        }
    }
}