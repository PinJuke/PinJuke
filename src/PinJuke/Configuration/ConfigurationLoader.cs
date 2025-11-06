using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PinJuke.Ini;
using PinJuke.Onboarding;

namespace PinJuke.Configuration
{
    public class ConfigurationLoader
    {
        protected readonly Parser parser = new();

        public Configuration FromIniFilePaths(IEnumerable<string> iniFilePaths, string? playlistConfigFilePath)
        {
            var iniDocument = new IniDocument();
            var iniReader = new IniReader();
            foreach (var iniFilePath in iniFilePaths)
            {
                iniDocument.MergeFrom(iniReader.Read(iniFilePath));
            }
            return FromIniDocument(iniDocument, playlistConfigFilePath);
        }

        public Configuration FromIniDocument(IniDocument iniDocument, string? playlistConfigFilePath = null)
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
            var cursorVisible = parser.ParseBool(iniDocument["PinJuke"]["CursorVisible"]) ?? false;
            return new Configuration(playlistConfigFilePath, mediaPath, player, keyboard, playField, backGlass, dmd, milkdrop, dof, cursorVisible);
        }

        /// <summary>
        /// At the moment only save what is changed during onboarding.
        /// Some paths are now absolute, they should not be saved.
        /// </summary>
        public void ToGlobalIniDocument(Configuration configuration, IniDocument iniDocument)
        {
            //iniDocument["PinJuke"]["MediaPath"] = parser.FormatString(configuration.MediaPath);

            SaveDisplay(iniDocument, "PlayField", configuration.PlayField);
            SaveDisplay(iniDocument, "BackGlass", configuration.BackGlass);
            iniDocument["DMD"]["Enabled"] = parser.FormatBool(configuration.Dmd.Enabled);
            SaveDisplay(iniDocument, "DMD", configuration.Dmd);

            //iniDocument["Keyboard"]["Exit"] = parser.FormatEnum<Key>(configuration.Keyboard.Exit);
            //iniDocument["Keyboard"]["Browse"] = parser.FormatEnum<Key>(configuration.Keyboard.Browse);
            //iniDocument["Keyboard"]["Previous"] = parser.FormatEnum<Key>(configuration.Keyboard.Previous);
            //iniDocument["Keyboard"]["Next"] = parser.FormatEnum<Key>(configuration.Keyboard.Next);
            //iniDocument["Keyboard"]["PlayPause"] = parser.FormatEnum<Key>(configuration.Keyboard.PlayPause);
            //iniDocument["Keyboard"]["VolumeDown"] = parser.FormatEnum<Key>(configuration.Keyboard.VolumeDown);
            //iniDocument["Keyboard"]["VolumeUp"] = parser.FormatEnum<Key>(configuration.Keyboard.VolumeUp);
            //iniDocument["Keyboard"]["Tilt"] = parser.FormatEnum<Key>(configuration.Keyboard.Tilt);

            //iniDocument["Milkdrop"]["PresetsPath"] = parser.FormatString(configuration.Milkdrop.PresetsPath);
            //iniDocument["Milkdrop"]["TexturesPath"] = parser.FormatString(configuration.Milkdrop.TexturesPath);

            iniDocument["DOF"]["Enabled"] = parser.FormatBool(configuration.Dof.Enabled);
            iniDocument["DOF"]["GlobalConfigFilePath"] = parser.FormatString(configuration.Dof.GlobalConfigFilePath);
            //iniDocument["DOF"]["RomName"] = parser.FormatString(configuration.Dof.RomName);
        }

        private void SaveDisplay(IniDocument iniDocument, string sectionName, Display display)
        {
            iniDocument[sectionName]["WindowLeft"] = parser.FormatInt(display.Window.Left);
            iniDocument[sectionName]["WindowTop"] = parser.FormatInt(display.Window.Top);
            iniDocument[sectionName]["WindowWidth"] = parser.FormatInt(display.Window.Width);
            iniDocument[sectionName]["WindowHeight"] = parser.FormatInt(display.Window.Height);
            iniDocument[sectionName]["ContentScale"] = parser.FormatFloat(display.Window.ContentScale);
            iniDocument[sectionName]["ContentRotation"] = parser.FormatInt(display.Window.ContentRotation);
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
                 GetRotation(displaySection["ContentRotation"]) ?? 0
            );

            var backgroundImageFile = ParseFullPathFromMediaPath(displaySection["BackgroundImageFile"], mediaPath);
            var themeVideoStartFile = ParseFullPathFromMediaPath(displaySection["ThemeVideoStartFile"], mediaPath);
            var themeVideoLoopFile = ParseFullPathFromMediaPath(displaySection["ThemeVideoLoopFile"], mediaPath);
            var themeVideoStopFile = ParseFullPathFromMediaPath(displaySection["ThemeVideoStopFile"], mediaPath);
            var themeVideoIdleFile = ParseFullPathFromMediaPath(displaySection["ThemeVideoIdleFile"], mediaPath);

            var content = new Content(
                parser.ParseBool(displaySection["CoverEnabled"]) ?? role == DisplayRole.DMD,
                parser.ParseBool(displaySection["StateEnabled"]) ?? true,
                parser.ParseBool(displaySection["BrowserEnabled"]) ?? role == DisplayRole.BackGlass,
                parser.ParseEnum<BackgroundType>(displaySection["PlaybackBackgroundType"]) ?? BackgroundType.MilkdropVisualization,
                parser.ParseEnum<BackgroundType>(displaySection["IdleBackgroundType"]) ?? BackgroundType.MilkdropVisualization,
                parser.ParseBool(displaySection["ThemeVideoStartFileEnabled"]) ?? false,
                parser.ParseBool(displaySection["ThemeVideoStopFileEnabled"]) ?? false,
                backgroundImageFile,
                themeVideoStartFile,
                themeVideoLoopFile,
                themeVideoStopFile,
                themeVideoIdleFile,
                GetRotation(displaySection["ThemeVideoRotation"]) ?? 0
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

        protected string ParseFullPathFromMediaPath(string? entryValue, string mediaPath)
        {
            var path = parser.ParseString(entryValue) ?? "";
            if (!path.IsNullOrEmpty())
            {
                path = GetFullPathFromMediaPath(path, mediaPath);
            }
            return path;
        }

        protected int? GetRotation(string? s)
        {
            var rotation = parser.ParseInt(s);
            if (rotation == null)
            {
                return null;
            }

            rotation = (int)Math.Round((double)rotation / 90.0) * 90;

            rotation = rotation % 360;
            if (rotation < 0)
            {
                rotation += 360;
            }

            return rotation;
        }
    }

}
