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
        private IniSection userSection;
        private Parser parser;

        private readonly OrderedDictionary<string, UserPlaylist> userPlaylists = new();
        private int nextIndex = 0;

        private bool setUp = false;
        public bool SetUp
        {
            get => setUp;
            set
            {
                setUp = value;
                userSection["SetUp"] = parser.FormatBool(value);
            }
        }

        private string? privateId = null;
        public string? PrivateId
        {
            get => privateId;
            set
            {
                privateId = value;
                userSection["PrivateId"] = parser.FormatString(value);
            }
        }

        private string? publicId = null;
        public string? PublicId
        {
            get => publicId;
            set
            {
                publicId = value;
                userSection["PublicId"] = parser.FormatString(value);
            }
        }

        private bool? updateCheckEnabled = null;
        public bool? UpdateCheckEnabled
        {
            get => updateCheckEnabled;
            set
            {
                updateCheckEnabled = value;
                userSection["UpdateCheckEnabled"] = parser.FormatBool(value);
            }
        }

        private bool? beaconEnabled = null;
        public bool? BeaconEnabled
        {
            get => beaconEnabled;
            set
            {
                beaconEnabled = value;
                userSection["BeaconEnabled"] = parser.FormatBool(value);
            }
        }

        private string? lastBeaconSentAt = null;
        public string? LastBeaconSentAt
        {
            get => lastBeaconSentAt;
            set
            {
                lastBeaconSentAt = value;
                userSection["LastBeaconSentAt"] = value;
            }
        }

        private string? developerName = null;
        public string? DeveloperName
        {
            get => developerName;
            set
            {
                developerName = value;
                userSection["DeveloperName"] = parser.FormatString(value);
            }
        }

        public UserConfiguration(IniDocument iniDocument, Parser parser, bool setUp = false, string? privateId = null, string? publicId = null, bool? updateCheckEnabled = null, bool? beaconEnabled = null, string? lastBeaconSentAt = null, string? developerName = null)
        {
            IniDocument = iniDocument;
            userSection = iniDocument["User"];
            this.parser = parser;
            SetUp = setUp;
            PrivateId = privateId;
            PublicId = publicId;
            UpdateCheckEnabled = updateCheckEnabled;
            BeaconEnabled = beaconEnabled;
            LastBeaconSentAt = lastBeaconSentAt;
            DeveloperName = developerName;
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
                var iniSection = IniDocument[name];
                playlist = new UserPlaylist(iniSection, parser, index, playlistConfigFilePath);
                AddPlaylist(playlist);
            }
            return playlist;
        }
    }

    public class UserPlaylist
    {
        public IniSection IniSection { get; }
        private Parser parser;
        public int Index { get; }

        private string playlistConfigFilePath = "";
        public string PlaylistConfigFilePath
        {
            get => playlistConfigFilePath;
            private set
            {
                playlistConfigFilePath = value;
                IniSection["PlaylistConfigFilePath"] = parser.FormatString(value);
            }
        }

        private string? trackFilePath = null;
        public string? TrackFilePath
        {
            get => trackFilePath;
            set
            {
                trackFilePath = value;
                IniSection["TrackFilePath"] = parser.FormatString(value);
            }
        }

        public UserPlaylist(IniSection iniSection, Parser parser, int index, string playlistConfigFilePath, string? trackFilePath = null)
        {
            IniSection = iniSection;
            this.parser = parser;
            Index = index;
            PlaylistConfigFilePath = playlistConfigFilePath;
            TrackFilePath = trackFilePath;
        }
    }
}
