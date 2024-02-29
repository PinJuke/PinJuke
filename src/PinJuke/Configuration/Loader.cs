using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinJuke.Configuration
{
    public class Loader
    {
        protected readonly Parser parser = new();

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
            // The media path should be a full path so that it can be used as a base path.
            mediaPath = GetFullPath(mediaPath);
            var player = CreatePlayer(iniDocument["Player"]);
            var keyboard = CreateKeyboard(iniDocument["Keyboard"]);
            var playField = CreateDisplay(DisplayRole.PlayField, iniDocument["PlayField"], mediaPath);
            var backGlass = CreateDisplay(DisplayRole.BackGlass, iniDocument["BackGlass"], mediaPath);
            var dmd = CreateDisplay(DisplayRole.DMD, iniDocument["DMD"], mediaPath);
            var milkdrop = CreateMilkdrop(iniDocument["Milkdrop"]);
            var dof = CreateDof(iniDocument["DOF"]);
            return new Configuration(mediaPath, player, keyboard, playField, backGlass, dmd, milkdrop, dof);
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
            var volumeDown = parser.ParseEnum<Key>(keyboardSection["VolumeDown"]) ?? Key.LeftCtrl;
            var volumeUp = parser.ParseEnum<Key>(keyboardSection["VolumeUp"]) ?? Key.RightCtrl;
            var tilt = parser.ParseEnum<Key>(keyboardSection["Tilt"]) ?? Key.T;
            return new Keyboard(exit, browse, previous, next, playPause, volumeDown, volumeUp, tilt);
        }

        protected Display CreateDisplay(DisplayRole role, IniSection displaySection, string mediaPath)
        {
            var enabled = role == DisplayRole.PlayField
                || role == DisplayRole.BackGlass
                || (parser.ParseBool(displaySection["Enabled"]) ?? false);

            var window = new Window(
                 parser.ParseInt(displaySection["WindowLeft"]) ?? 0,
                 parser.ParseInt(displaySection["WindowTop"]) ?? 0,
                 parser.ParseInt(displaySection["WindowWidth"]) ?? 400,
                 parser.ParseInt(displaySection["WindowHeight"]) ?? 300,
                 parser.ParseFloat(displaySection["ContentScale"]) ?? 1,
                 GetAngle(displaySection["ContentAngle"]) ?? 0
            );

            var backgroundImageFile = parser.ParseString(displaySection["BackgroundImageFile"]) ?? "";
            if (!backgroundImageFile.IsNullOrEmpty())
            {
                backgroundImageFile = GetFullPathFromMediaPath(backgroundImageFile, mediaPath);
            }
            var songStartFile = parser.ParseString(displaySection["SongStartFile"]) ?? "";
            if (!songStartFile.IsNullOrEmpty())
            {
                songStartFile = GetFullPathFromMediaPath(songStartFile, mediaPath);
            }
            var content = new Content(
                parser.ParseEnum<BackgroundType>(displaySection["BackgroundType"]) ?? BackgroundType.MilkdropVisualization,
                backgroundImageFile,
                parser.ParseBool(displaySection["CoverEnabled"]) ?? role == DisplayRole.DMD,
                parser.ParseBool(displaySection["StateEnabled"]) ?? true,
                parser.ParseBool(displaySection["BrowserEnabled"]) ?? true,
                songStartFile
            );

            return new Display(role, enabled, window, content);
        }

        protected Milkdrop CreateMilkdrop(IniSection milkdropSection)
        {
            var presetsPath = parser.ParseString(milkdropSection["PresetsPath"]) ?? ".";
            presetsPath = GetFullPath(presetsPath);
            var texturesPath = parser.ParseString(milkdropSection["TexturesPath"]) ?? ".";
            texturesPath = GetFullPath(texturesPath);

            return new Milkdrop(presetsPath, texturesPath);
        }

        protected Dof CreateDof(IniSection dofSection)
        {
            var enabled = parser.ParseBool(dofSection["Enabled"]) ?? false;
            var globalConfigFilePath = parser.ParseString(dofSection["GlobalConfigFilePath"]) ?? "";
            var romName = parser.ParseString(dofSection["RomName"]) ?? "pinjuke";
            if (globalConfigFilePath.IsNullOrEmpty() || romName.IsNullOrEmpty())
            {
                enabled = false;
            }
            else
            {
                globalConfigFilePath = GetFullPath(globalConfigFilePath);
            }
            return new Dof(
                enabled,
                globalConfigFilePath,
                romName
            );
        }

        protected string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        protected string GetFullPathFromMediaPath(string path, string mediaPath)
        {
            return Path.GetFullPath(path, mediaPath);
        }

        protected int? GetAngle(string? s)
        {
            var angle = parser.ParseInt(s);
            if (angle == null)
            {
                return null;
            }

            angle = (int)Math.Round((double)angle / 90.0) * 90;

            angle = angle % 360;
            if (angle < 0)
            {
                angle += 360;
            }

            return angle;
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

        public float? ParseFloat(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
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
