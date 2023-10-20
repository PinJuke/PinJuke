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
    record Configuration(string MediaPath, Player Player, Keyboard Keyboard, Display BackGlass, Display PlayField, Display DMD);

    record Player(string MusicPath, StartupTrackType StartupTrackType, bool PlayOnStartup);

    enum StartupTrackType
    {
        LastPlayedTrack = 0,
        FirstTrack = 1,
        Random = 2
    }

    record Display(bool Enabled, Window Window, Content Content);

    record Window(int Left, int Top, int Width, int Height, int Orientation);

    record Content(BackgroundType BackgroundType, string BackgroundImageFile, bool BrowserEnabled, string SongStartFile);

    enum BackgroundType
    {
        Image = 0,
        MilkdropVisualization = 1,
    }

    record Keyboard(Key Exit, Key Browse, Key Previous, Key Next, Key PlayPause);

}
