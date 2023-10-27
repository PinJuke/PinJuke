using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinJuke.Configuration
{
    public record Configuration(string MediaPath, Player Player, Keyboard Keyboard, Display BackGlass, Display PlayField, Display DMD);

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
        BackGlass = 0,
        PlayField = 1,
        DMD = 2,
    }

    public record Window(int Left, int Top, int Width, int Height, float ContentScale, int ContentAngle);

    public record Content(BackgroundType BackgroundType, string BackgroundImageFile, bool BrowserEnabled, string SongStartFile);

    public enum BackgroundType
    {
        Image = 0,
        MilkdropVisualization = 1,
    }

    public record Keyboard(Key Exit, Key Browse, Key Previous, Key Next, Key PlayPause);

}
