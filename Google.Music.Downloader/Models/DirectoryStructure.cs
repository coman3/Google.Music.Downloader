using System;
using System.IO;
using System.Text.RegularExpressions;
using GoogleMusicApi.Structure;

namespace Google.Music.Downloader.Models
{
    internal abstract class DirectoryStructure
    {
        public string BaseDirectory { get; set; }

        public abstract string FriendlyName { get; }

        protected DirectoryStructure(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
        }

        public virtual string GetFilePath(string playlistName, Track song)
        {
            return Path.Combine(GetFileFolder(playlistName, song), CleanInput(song.Title) + ".mp3");
        }

        public abstract string GetFileFolder(string playlist, Track song);

        protected static string CleanInput(string strIn)
        {
            try
            {
                var str = Regex.Replace(strIn, @" ?\(.*?\)", "", RegexOptions.None, TimeSpan.FromSeconds(0.5));
                str = Regex.Replace(str, @"[^a-zA-Z0-9 -]", "", RegexOptions.None, TimeSpan.FromSeconds(0.5));


                return str;
            }
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
            }
        }
    }

    internal sealed class PlaylistDirectoryStructure : DirectoryStructure
    {
        public PlaylistDirectoryStructure(string baseDirectory) : base(baseDirectory)
        {
        }

        public override string FriendlyName => "Playlist Folder Structure";

        public override string GetFileFolder(string playlist, Track song)
        {
            return Path.Combine(BaseDirectory, CleanInput(playlist));
        }
    }
    internal sealed class GenreDirectoryStructure : DirectoryStructure
    {
        public GenreDirectoryStructure(string baseDirectory) : base(baseDirectory)
        {
        }

        public override string FriendlyName => "Genre Folder Structure";

        public override string GetFileFolder(string playlist, Track song)
        {
            return Path.Combine(BaseDirectory, CleanInput(song.Genre.Replace('/', '-')));
        }
    }

    internal sealed class ArtistDirectoryStructure : DirectoryStructure
    {
        public override string FriendlyName => "Artist Folder Structure";

        public ArtistDirectoryStructure(string baseDirectory) : base(baseDirectory)
        {
        }

        public override string GetFileFolder(string playlist, Track song)
        {
            return Path.Combine(BaseDirectory, CleanInput(song.Artist));
        }
    }

    internal sealed class NoDirectoryStructure : DirectoryStructure
    {
        public override string FriendlyName => "Song Title File Structure";

        public override string GetFileFolder(string playlist, Track song)
        {
            return BaseDirectory;
        }

        public NoDirectoryStructure(string baseDirectory) : base(baseDirectory)
        {
        }
    }

    internal sealed class AlbumDirectoryStructure : DirectoryStructure
    {
        public override string FriendlyName => "Album Folder Structure";

        public override string GetFileFolder(string playlist, Track song)
        {
            return Path.Combine(BaseDirectory, CleanInput(song.Album));
        }

        public AlbumDirectoryStructure(string baseDirectory) : base(baseDirectory)
        {
        }
    }
}