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
using Google.Music.Downloader.Models;

namespace Google.Music.Downloader.Pages
{
    /// <summary>
    /// Interaction logic for BrowsePage.xaml
    /// </summary>
    public partial class BrowsePage : Page
    {
        public List<Playlist> Playlists { get; set; }
        public BrowsePage()
        {
            InitializeComponent();

            Load();

        }

        private async void Load()
        {
            Playlists = new List<Playlist>();
            var playlists = await ApplicationState.MobileClient.ListPlaylistsAsync();
            foreach (var playlist in playlists.Data.Items.Where(x=> !x.Deleted))
            {
                Playlists.Add(new StandardPlaylist(playlist));
            }

            var topCharts = await ApplicationState.MobileClient.ListPromotedTracksAsync();
            Playlists.Add(new CustomPlaylist("Top Charts", topCharts.Chart.Tracks));

            var thumbsUp = await ApplicationState.MobileClient.ListThumbsUpTracksAsync();
            Playlists.Add(new CustomPlaylist("Thumbs Up", thumbsUp.Data.Items));
            if(ListBox.ItemsSource == null)
                ListBox.Items.Clear();
            ListBox.ItemsSource = Playlists;
        }

        private void ListBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBox.SelectedItem != null && ListBox.SelectedItem is Playlist)
            {
                var playlist = (Playlist) ListBox.SelectedItem;
                ApplicationState.SetPage(new PlaylistPage(playlist));
            }
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox.SelectedItems != null)
            {
                ApplicationState.SetPage(new PlaylistPage(ListBox.SelectedItems.Cast<Playlist>().ToArray()));
            }
        }

        private void ButtonReLoad_Click(object sender, RoutedEventArgs e)
        {
            ListBox.ItemsSource = null;
            ListBox.Items.Add("Loading...");
            Load();
        }
    }
}
