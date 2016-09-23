using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using GoogleMusicApi.Structure;
using Playlist = Google.Music.Downloader.Models.Playlist;

namespace Google.Music.Downloader.Pages
{
    /// <summary>
    /// Interaction logic for PlaylistPage.xaml
    /// </summary>
    public partial class PlaylistPage : Page
    {
        public Playlist[] Playlists { get; set; }
        public ObservableCollection<DataTrack> DataTracks { get; set; }

        public PlaylistPage(params Playlist[] playlists)
        {
            InitializeComponent();
            Load(playlists);

        }

        private async void Load(Playlist[] playlists)
        {
            Playlists = playlists;
            DataTracks = new ObservableCollection<DataTrack>();
            foreach (var playlist in playlists)
            {
                await playlist.LoadAsync(ApplicationState.MobileClient);
                foreach (var playlistTrack in playlist.Tracks)
                {
                    DataTracks.Add(new DataTrack(playlistTrack, playlist.Name));
                }
            }
            
            Data.ItemsSource = DataTracks;
        }

        private void ButtonDownload_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationState.SetPage(new DownloadPage(Playlists));
        }

        private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationState.SetPage(new BrowsePage());
        }

        private void MenuItem_Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItems = Data.SelectedItems.Cast<DataTrack>().ToList();
            foreach (var playlist in Playlists)
            {
                var tracks = playlist.Tracks.ToList();
                tracks.RemoveAll(x => selectedItems.Any(c => c.Track == x));
                playlist.Tracks = tracks.ToArray();
            }

            selectedItems.ForEach(x=> DataTracks.Remove(x));
        }

        private void Data_OnUnloadingRow(object sender, DataGridRowEventArgs e)
        {
            foreach (var playlist in Playlists)
            {
                var tracks = playlist.Tracks.ToList();
                tracks.Remove(((DataTrack) e.Row.Item).Track);
                playlist.Tracks = tracks.ToArray();
            }
        }
    }

    public class DataTrack
    {
        public string Title { get { return Track.Title; } set { Track.Title = value; } }
        public string Album { get { return Track.Album; } set { Track.Album = value; } }
        public string Artist { get { return Track.Artist; } set { Track.Artist = value; } }
        public string Genre { get { return Track.Genre; } set { Track.Genre = value; } }
        public string Playlist { get; private set; }
        internal Track Track { get; set; }
        public DataTrack(Track track, string playlistName)
        {
            Playlist = playlistName;
            Track = track;
        }
    }
}
