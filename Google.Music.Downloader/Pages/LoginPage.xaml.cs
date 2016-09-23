using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Google.Music.Downloader.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void ButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            button.IsEnabled = false;
            if (await ApplicationState.MobileClient.LoginAsync(Username.Text, Password.Password))
            {
                ApplicationState.SetPage(new BrowsePage());
                return;
            }

            MessageBox.Show(ApplicationState.MainWindow,
                "Login Failed!\n" +
                "Make Sure you do not have 2 factor auth setup and check your username and password again.",
                "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            button.IsEnabled = true;

        }
    }
}
