using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace PinJuke.Configuration
{
    public class Loader
    {
        private readonly Parser parser = new();

        public Configuration FromIniFilePaths(IEnumerable<string> iniFilePaths)
        {
            var iniDocument = new IniDocument();
            var iniReader = new IniReader();
            foreach (var iniFilePath in iniFilePaths)
            {
                iniDocument.MergeFrom(iniReader.Read(iniFilePath));
            }
            return FromIniDocument(iniDocument);
        }

        public Configuration FromIniDocument(IniDocument iniDocument)
        {
            var mediaPath = parser.ParseString(iniDocument["PinJuke"]["MediaPath"]) ?? ".";
            var player = CreatePlayer(iniDocument["Player"]);
            var keyboard = CreateKeyboard(iniDocument["Keyboard"]);
            var backGlass = CreateDisplay(DisplayRole.BackGlass, iniDocument["BackGlass"]);
            var playField = CreateDisplay(DisplayRole.PlayField, iniDocument["PlayField"]);
            var dmd = CreateDisplay(DisplayRole.DMD, iniDocument["DMD"]);
            return new Configuration(mediaPath, player, keyboard, backGlass, playField, dmd);
        }

        protected Player CreatePlayer(IniSection playerSection)
        {
            var musicPath = parser.ParseString(playerSection["MusicPath"])
                ?? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            var startupTrackType = parser.ParseEnum<StartupTrackType>(playerSection["StartupTrackType"])
                ?? StartupTrackType.FirstTrack;
            var playOnStartup = parser.ParseBool(playerSection["PlayOnStartup"])
                ?? true;
            return new Player(musicPath, startupTrackType, playOnStartup);
        }

        protected Keyboard CreateKeyboard(IniSection keyboardSection)
        {
            var exit = parser.ParseEnum<Key>(keyboardSection["Exit"]) ?? Key.Escape;
            var browse = parser.ParseEnum<Key>(keyboardSection["Browse"]) ?? Key.Enter;
            var previous = parser.ParseEnum<Key>(keyboardSection["Previous"]) ?? Key.LeftShift;
            var next = parser.ParseEnum<Key>(keyboardSection["Next"]) ?? Key.RightShift;
            var playPause = parser.ParseEnum<Key>(keyboardSection["PlayPause"]) ?? Key.D1;
            return new Keyboard(exit, browse, previous, next, playPause);
        }

        protected Display CreateDisplay(DisplayRole role, IniSection displaySection)
        {
            var enabled = parser.ParseBool(displaySection["Enabled"]) ?? true;

            var orientation = parser.ParseInt(displaySection["WindowOrientation"]) ?? 0;
            orientation = orientation % 360;
            if (orientation < 0)
            {
                orientation += 360;
            }
            if (orientation % 90 != 0)
            {
                orientation = 0;
            }

            var window = new Window(
                 parser.ParseInt(displaySection["WindowLeft"]) ?? 0,
                 parser.ParseInt(displaySection["WindowTop"]) ?? 0,
                 parser.ParseInt(displaySection["WindowWidth"]) ?? 400,
                 parser.ParseInt(displaySection["WindowHeight"]) ?? 300,
                 orientation
            );

            var content = new Content(
                parser.ParseEnum<BackgroundType>(displaySection["BackgroundType"]) ?? BackgroundType.MilkdropVisualization,
                parser.ParseString(displaySection["BackgroundImageFile"]) ?? "",
                parser.ParseBool(displaySection["BrowserEnabled"]) ?? true,
                parser.ParseString(displaySection["BackgroundImageFile"]) ?? ""
            );

            return new Display(role, enabled, window, content);
        }
    }

    public class Parser
    {
        /// <summary>
        /// Should at least check for null.
        /// </summary>
        public bool IsUndefined(string? s)
        {
            return s == null;
        }

        public string? ParseString(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            return s;
        }

        public int? ParseInt(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return null;
        }

        public bool? ParseBool(string? s)
        {
            var i = ParseInt(s);
            if (i == null)
            {
                return null;
            }
            return i != 0;
        }

        public TEnum? ParseEnum<TEnum>(string? s) where TEnum : struct
        {
            if (IsUndefined(s))
            {
                return null;
            }
            if (Enum.TryParse<TEnum>(s, out var result))
            {
                return result;
            }
            return null;
        }

    }

}
