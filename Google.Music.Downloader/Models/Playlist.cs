using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GoogleMusicApi.Common;
using GoogleMusicApi.Structure;

namespace Google.Music.Downloader.Models
{
    public abstract class Playlist
    {
        public string Name { get; set; }
        public Track[] Tracks { get; set; }

        protected Playlist(string name)
        {
            Name = name;
        }

#pragma warning disable 1998
        public virtual async Task LoadAsync(MobileClient client)
#pragma warning restore 1998
        {
        }

        public override string ToString()
        {
            return $"Playlist: {Name}";
        }
    }

    public sealed class StandardPlaylist : Playlist
    {
        public GoogleMusicApi.Structure.Playlist Playlist { get; set; }

        public StandardPlaylist(GoogleMusicApi.Structure.Playlist playlist) : base(playlist.Name)
        {
            Playlist = playlist;
        }

        public override async Task LoadAsync(MobileClient client)
        {
            var tracks = await client.ListTracksFromPlaylist(Playlist);
            Tracks = tracks.ToArray();
        }

    }

    public sealed class CustomPlaylist : Playlist
    {

        public CustomPlaylist(string name, IEnumerable<Track> tracks) : base(name)
        {
            Tracks = tracks.ToArray();
        }
        public override string ToString()
        {
            return $"Auto Playlist: {Name}";
        }
    }
}