using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinJuke.Configuration
{
    public record Configuration(string? PlaylistConfigFilePath, string MediaPath, Player Player, Keyboard Keyboard, Display PlayField, Display BackGlass, Display Dmd, Milkdrop Milkdrop, Dof Dof);

    public record Player(string MusicPath, StartupTrackType StartupTrackType, bool PlayOnStartup);

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

    public record Content(BackgroundType BackgroundType, string BackgroundImageFile, bool CoverEnabled, bool StateEnabled, bool BrowserEnabled, string ThemeVideoStartFile, string ThemeVideoLoopFile, string ThemeVideoStopFile, int ThemeVideoRotation);

    public enum BackgroundType
    {
        Image = 0,
        MilkdropVisualization = 1,
    }

    public record Keyboard(Key Exit, Key Browse, Key Previous, Key Next, Key PlayPause, Key VolumeDown, Key VolumeUp, Key Tilt);

    public record Milkdrop(string PresetsPath, string TexturesPath);

    public record Dof(bool Enabled, string GlobalConfigFilePath, string RomName);

}
