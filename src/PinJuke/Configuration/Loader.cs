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
            var player = CreatePlayer(iniDocument["Player"]);
            var keyboard = CreateKeyboard(iniDocument["Keyboard"]);
            var backGlass = CreateDisplay(DisplayRole.BackGlass, iniDocument["BackGlass"], mediaPath);
            var playField = CreateDisplay(DisplayRole.PlayField, iniDocument["PlayField"], mediaPath);
            var dmd = CreateDisplay(DisplayRole.DMD, iniDocument["DMD"], mediaPath);
            var milkdrop = CreateMilkdrop(iniDocument["Milkdrop"], mediaPath);
            var dof = CreateDof(iniDocument["DOF"]);
            return new Configuration(mediaPath, player, keyboard, backGlass, playField, dmd, milkdrop, dof);
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

        protected Display CreateDisplay(DisplayRole role, IniSection displaySection, string mediaPath)
        {
            var enabled = parser.ParseBool(displaySection["Enabled"]) ?? true;

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
                backgroundImageFile = GetFullPath(backgroundImageFile, mediaPath);
            }
            var songStartFile = parser.ParseString(displaySection["SongStartFile"]) ?? "";
            if (!songStartFile.IsNullOrEmpty())
            {
                songStartFile = GetFullPath(songStartFile, mediaPath);
            }
            var content = new Content(
                parser.ParseEnum<BackgroundType>(displaySection["BackgroundType"]) ?? BackgroundType.MilkdropVisualization,
                backgroundImageFile,
                parser.ParseBool(displaySection["CoverEnabled"]) ?? role == DisplayRole.DMD,
                parser.ParseBool(displaySection["BrowserEnabled"]) ?? true,
                songStartFile
            );

            return new Display(role, enabled, window, content);
        }

        protected Milkdrop CreateMilkdrop(IniSection milkdropSection, string mediaPath)
        {
            var presetsPath = parser.ParseString(milkdropSection["PresetsPath"]) ?? ".";
            presetsPath = GetFullPath(presetsPath, mediaPath);
            var texturesPath = parser.ParseString(milkdropSection["TexturesPath"]) ?? ".";
            texturesPath = GetFullPath(texturesPath, mediaPath);

            return new Milkdrop(presetsPath, texturesPath);
        }

        protected Dof CreateDof(IniSection dofSection)
        {
            var enabled = parser.ParseBool(dofSection["Enabled"]) ?? false;
            var globalConfigFilePath = parser.ParseString(dofSection["GlobalConfigFilePath"]) ?? "";
            var romName = parser.ParseString(dofSection["RomName"]) ?? "";
            if (globalConfigFilePath.IsNullOrEmpty() || romName.IsNullOrEmpty())
            {
                enabled = false;
            }
            return new Dof(
                enabled,
                globalConfigFilePath,
                romName
            );
        }

        protected string GetFullPath(string path, string mediaPath)
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
