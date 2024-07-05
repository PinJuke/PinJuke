using PinJuke.Ini;
using SoftCircuits.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Configuration
{
    public class UserConfiguration
    {
        public IniDocument IniDocument { get; }
        private readonly OrderedDictionary<string, UserPlaylist> userPlaylists = new();
        private int nextIndex = 0;

        public UserConfiguration(IniDocument iniDocument)
        {
            IniDocument = iniDocument;
        }

        public void AddPlaylist(UserPlaylist playlist)
        {
            userPlaylists[playlist.PlaylistConfigFilePath] = playlist;
            if (playlist.Index >= nextIndex)
            {
                nextIndex = playlist.Index + 1;
            }
        }

        public UserPlaylist ProvidePlaylist(string playlistConfigFilePath)
        {
            var playlist = userPlaylists.GetValueOrDefault(playlistConfigFilePath);
            if (playlist == null)
            {
                var index = nextIndex;
                var name = "Playlist" + index;
                var iniSection = IniDocument.ProvideSection(name);
                playlist = new UserPlaylist(iniSection, index, playlistConfigFilePath);
                AddPlaylist(playlist);
            }
            return playlist;
        }
    }

    public class UserPlaylist
    {
        public IniSection IniSection { get; }
        public int Index { get; }

        private string playlistConfigFilePath = "";
        public string PlaylistConfigFilePath
        {
            get => playlistConfigFilePath;
            private set
            {
                playlistConfigFilePath = value;
                IniSection["PlaylistConfigFilePath"] = value;
            }
        }

        private string trackFilePath = "";
        public string TrackFilePath
        {
            get => trackFilePath;
            set
            {
                trackFilePath = value;
                IniSection["TrackFilePath"] = value;
            }
        }

        public UserPlaylist(IniSection iniSection, int index, string playlistConfigFilePath)
        {
            IniSection = iniSection;
            Index = index;
            PlaylistConfigFilePath = playlistConfigFilePath;
            TrackFilePath = "";
        }
    }
}
