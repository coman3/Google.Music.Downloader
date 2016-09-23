using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Music.Downloader.Models;
using GoogleMusicApi.Structure;
using TagLib;
using Button = System.Windows.Controls.Button;
using File = System.IO.File;
using MessageBox = System.Windows.MessageBox;
using Playlist = Google.Music.Downloader.Models.Playlist;
using TextBox = System.Windows.Controls.TextBox;


namespace Google.Music.Downloader.Pages
{
    /// <summary>
    /// Interaction logic for DownloadPage.xaml
    /// </summary>
    public partial class DownloadPage : Page
    {
        public string SavePath => DirectoryTextBox.Text;
        public int Downloaded { get; set; }
        public int TotalToDownload => Playlists.Sum(x => x.Tracks.Length);
        private Dictionary<string, DirectoryStructure> DirectoryStructures { get; set; }
        public Playlist[] Playlists { get; set; }
        public DownloadPage(Playlist[] playlists)
        {
            InitializeComponent();
            Playlists = playlists;
            DirectoryStructures = new Dictionary<string, DirectoryStructure>
            {
                ["Playlist"] = new PlaylistDirectoryStructure(null),
                ["None"] = new NoDirectoryStructure(null),
                ["Album"] = new AlbumDirectoryStructure(null),
                ["Artist"] = new ArtistDirectoryStructure(null),
                ["Genre"] = new GenreDirectoryStructure(null)
            };
            DirectoryStructure.ItemsSource = DirectoryStructures.Keys;    
        }
        private async Task DownloadTracks(DirectoryStructure structure, string friendlyName, Track[] tracks)
        {
            
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            Console.WriteLine("Downloading Playlist: " + friendlyName);
            try
            {

                foreach (var song in tracks)
                {
                    try
                    {
                        var newFilePath = structure.GetFilePath(friendlyName, song);
                        Console.WriteLine("     Downloading Song: " + song.Title + " - " + song.Artist);

                        //Create Directory Structure
                        if (!Directory.Exists(structure.GetFileFolder(friendlyName, song)))
                            Directory.CreateDirectory(structure.GetFileFolder(friendlyName, song));

                        if (File.Exists(newFilePath)) //File Exists, skip to next song.
                            continue;

                        //Download Track
                        var streamUrl = await ApplicationState.MobileClient.GetStreamUrlAsync(song);
                        using (var client = new WebClient())
                        {
                            await client.DownloadFileTaskAsync(streamUrl, newFilePath);
                        }

                        // Edit File MetaData
                        var fileInfo = new FileInfo(newFilePath);
                        using (var fileStream = File.Open(newFilePath, FileMode.Open, FileAccess.ReadWrite))
                        {
                            var file =
                                TagLib.File.Create(new StreamFileAbstraction(fileInfo.Name, fileStream, fileStream));
                            var tags = file.Tag;
                            tags.Title = song.Title;
                            tags.Album = song.Album;
                            tags.AlbumSort = song.DiscNumber.ToString();
                            tags.AlbumArtists = new[] { song.Artist };
                            tags.Track = Convert.ToUInt32(song.TrackNumber);
                            tags.Year = Convert.ToUInt32(song.Year);

                            file.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("     Failed Downloading Song: " + song.Title + " - " + song.Artist);
                        Console.WriteLine(ex.Message);
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    finally
                    {
                        ProgressBar.Value++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("     Downloaded Song: " + song.Title + " - " + song.Artist);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Downloaded Playlist: " + friendlyName);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed Downloading Playlist: " + friendlyName);
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void ButtonBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                DirectoryTextBox.Text = dialog.SelectedPath;
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (DirectoryStructure.SelectedItem == null || !Directory.Exists(DirectoryTextBox.Text))
            {
                MessageBox.Show(ApplicationState.MainWindow,
                    "Please select the directory to save the music " +
                    "and the directory structure in which to save it.",
                    "Enter required data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var button = (Button) sender;
            button.IsEnabled = false;

            CloseButton.Content = "Cancel and Close";
            StartOverButton.Content = "Cancel and Start Over";

            ProgressBar.Maximum = TotalToDownload;
            Console.SetOut(new ControlWriter(LogBox));

            var structure = DirectoryStructures[(string) DirectoryStructure.SelectedItem];
            structure.BaseDirectory = SavePath;
            foreach (var playlist in Playlists)
            {
                await DownloadTracks(structure, playlist.Name, playlist.Tracks);
            }

            Console.WriteLine("Finished!");
            var result = MessageBox.Show(ApplicationState.MainWindow, 
                "Finished Downloading Songs!\n" +
                "Would you like to close?", "Finished Downloading",
                MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                ApplicationState.MainWindow.Close();
            }
            else if (result == MessageBoxResult.No)
            {
                ApplicationState.SetPage(new BrowsePage());
            }

            Console.SetOut(Console.Out);

            CloseButton.Content = "Close";
            StartOverButton.Content = "Start Over";
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationState.MainWindow.Close();
        }

        private void ButtonStartOver_OnClick(object sender, RoutedEventArgs e)
        {
            ApplicationState.SetPage(new BrowsePage());
        }
    }
    public class ControlWriter : TextWriter
    {
        private readonly TextBox _textbox;
        public ControlWriter(TextBox textbox)
        {
            _textbox = textbox;
        }

        public override void Write(char value)
        {
            _textbox.Text = value + _textbox.Text;
        }

        public override void Write(string value)
        {
            _textbox.Text = value + _textbox.Text;
        }

        public override void WriteLine(string value)
        {
            _textbox.Text = value + "\n" + _textbox.Text;
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}
