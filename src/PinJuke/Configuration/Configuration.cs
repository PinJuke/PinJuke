using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinJuke.Configuration
{
    public record Configuration(string? PlaylistConfigFilePath, string MediaPath, Player Player, Keyboard Keyboard, Controller? Controller, Display PlayField, Display BackGlass, Display Dmd, Milkdrop Milkdrop, Dof Dof, Spotify.SpotifyConfig Spotify, bool CursorVisible);

    public record Player(PlayerSourceType SourceType, string MusicPath, string SpotifyPlaylistId, StartupTrackType StartupTrackType, bool PlayOnStartup, bool ShufflePlaylist);

    public enum PlayerSourceType
    {
        LocalFiles = 0,
        SpotifyPlaylist = 1,
    }

    public enum StartupTrackType
    {
        LastPlayedTrack = 0,
        FirstTrack = 1,
        Random = 2,
    }

    public record Display(DisplayRole Role, bool Enabled, Window Window, Content Content);

    public enum DisplayRole
    {
        PlayField = 0,
        BackGlass = 1,
        DMD = 2,
    }

    public record Window(int Left, int Top, int Width, int Height, float ContentScale, int ContentRotation);

    public record Content(
        bool CoverEnabled,
        bool StateEnabled,
        bool BrowserEnabled,
        BackgroundType PlaybackBackgroundType,
        BackgroundType IdleBackgroundType,
        bool ThemeVideoStartFileEnabled,
        bool ThemeVideoStopFileEnabled,
        string BackgroundImageFile,
        string ThemeVideoStartFile,
        string ThemeVideoLoopFile,
        string ThemeVideoStopFile,
        string ThemeVideoIdleFile,
        int ThemeVideoRotation
    );

    public enum BackgroundType
    {
        Image = 0,
        MilkdropVisualization = 1,
        Video = 2,
    }

    public record Keyboard(Key Exit, Key Browse, Key Previous, Key Next, Key PlayPause, Key VolumeDown, Key VolumeUp, Key Tilt);

    public record Controller(int Exit, int Browse, int Previous, int Next, int PlayPause, int VolumeDown, int VolumeUp, int Tilt);

    public record Milkdrop(string PresetsPath, string TexturesPath);

    public record Dof(bool Enabled, string GlobalConfigFilePath, string RomName);

}
