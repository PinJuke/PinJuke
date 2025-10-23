using PinJuke.Configuration;
using PinJuke.Configurator.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinJuke.Configurator.Factory
{
    public class PlaylistGroupControlFactory : GroupControlFactory
    {
        public PlaylistGroupControlFactory(Parser parser, MediaPathProvider mediaPathProvider)
        {
            LabelText = Strings.PlaylistConfiguration;
            Controls = [
                new GroupControlFactory()
                {
                    LabelText = Strings.Player,
                    Controls = [
                        new PathControlFactory()
                        {
                            LabelText = Strings.MusicPath,
                            FileMode = false,
                            RelativeEnabled = false,
                            Converter = new PathConverter(parser, "Player", "MusicPath"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.StartupTrackType,
                            Items = new()
                            {
                                new(Strings.StartupTrackTypeLastPlayedTrack, 0),
                                new(Strings.StartupTrackTypeFirstTrack, 1),
                                new(Strings.StartupTrackTypeRandomMode, 2),
                            },
                            Converter = new IntSelectConverter(parser, "Player", "StartupTrackType"),
                        },
                        new BoolControlFactory()
                        {
                            LabelText = Strings.PlayOnStartup,
                            Converter = new BoolConverter(parser, "Player", "PlayOnStartup"),
                        },
                    ]
                },
                new ContentGroupControlFactory(parser, "PlayField", mediaPathProvider)
                {
                    LabelText = Strings.DisplayPlayField,
                },
                new ContentGroupControlFactory(parser, "BackGlass", mediaPathProvider)
                {
                    LabelText = Strings.DisplayBackGlass,
                },
                new ContentGroupControlFactory(parser, "DMD", mediaPathProvider)
                {
                    LabelText = Strings.DisplayDmd,
                },
            ];
        }
    }

    public class ContentGroupControlFactory : GroupControlFactory
    {
        public const string BACKGROUND_IMAGE_FILE_CONTROL = "BackgroundImageFile";
        public const string THEME_VIDEO_START_FILE_CONTROL = "ThemeVideoStartFile";
        public const string THEME_VIDEO_LOOP_FILE_CONTROL = "ThemeVideoLoopFile";
        public const string THEME_VIDEO_STOP_FILE_CONTROL = "ThemeVideoStopFile";
        public const string THEME_VIDEO_ROTATION_CONTROL = "ThemeVideoRotation";

        public ContentGroupControlFactory(Parser parser, string sectionName, MediaPathProvider mediaPathProvider)
        {
            Controls = [
                new SelectControlFactory()
                {
                    LabelText = Strings.BackgroundType,
                    Items = new()
                    {
                        new(Strings.BackgroundTypeShowSpecifiedImage, 0),
                        new(Strings.BackgroundTypeShowMilkdropVisualizations, 1),
                    },
                    Converter = new IntSelectConverter(parser, sectionName, "BackgroundType"),
                    ChangedHandler = (ConfiguratorControl control) =>
                    {
                        var value = ((SelectControl)control).SelectedValue;
                        var enabled = value is int intValue && intValue == 0;
                        var group = control.GetParentGroup();
                        ((PathControl)group.GetChildByName(BACKGROUND_IMAGE_FILE_CONTROL)).Enabled = enabled;
                    },
                },
                new PathControlFactory()
                {
                    Name = BACKGROUND_IMAGE_FILE_CONTROL,
                    LabelText = Strings.BackgroundImageFile,
                    FileMode = true,
                    RelativeEnabled = true,
                    FileExtension = ".jpg",
                    FileFilter = $"{Strings.JpegFile}|*.jpg;*.jpeg;*.jpe;*.jfif",
                    MediaPathProvider = mediaPathProvider,
                    Converter = new PathConverter(parser, sectionName, "BackgroundImageFile"),
                },
                new BoolControlFactory()
                {
                    LabelText = Strings.EnableTrackCover,
                    Converter = new BoolConverter(parser, sectionName, "CoverEnabled"),
                },
                new BoolControlFactory()
                {
                    LabelText = Strings.EnablePlaybackStatus,
                    Converter = new BoolConverter(parser, sectionName, "StateEnabled"),
                },
                new BoolControlFactory()
                {
                    LabelText = Strings.EnableTrackBrowser,
                    Converter = new BoolConverter(parser, sectionName, "BrowserEnabled"),
                },
                new BoolControlFactory()
                {
                    LabelText = Strings.EnableThemeVideo,
                    Converter = new BoolConverter(parser, sectionName, "ThemeVideoEnabled"),
                    ChangedHandler = (ConfiguratorControl control) =>
                    {
                        var enabled = ((BoolControl)control).Value;
                        var group = control.GetParentGroup();
                        ((PathControl)group.GetChildByName(THEME_VIDEO_START_FILE_CONTROL)).Enabled = enabled;
                        ((PathControl)group.GetChildByName(THEME_VIDEO_LOOP_FILE_CONTROL)).Enabled = enabled;
                        ((PathControl)group.GetChildByName(THEME_VIDEO_STOP_FILE_CONTROL)).Enabled = enabled;
                        ((SelectControl)group.GetChildByName(THEME_VIDEO_ROTATION_CONTROL)).Enabled = enabled;
                    },
                },
                new PathControlFactory()
                {
                    Name = THEME_VIDEO_START_FILE_CONTROL,
                    LabelText = Strings.ThemeVideoStartFile,
                    FileMode = true,
                    RelativeEnabled = true,
                    FileExtension = ".mp4",
                    FileFilter = $"{Strings.Mp4File}|*.mp4",
                    MediaPathProvider = mediaPathProvider,
                    Converter = new PathConverter(parser, sectionName, "ThemeVideoStartFile"),
                },
                new PathControlFactory()
                {
                    Name = THEME_VIDEO_LOOP_FILE_CONTROL,
                    LabelText = Strings.ThemeVideoLoopFile,
                    FileMode = true,
                    RelativeEnabled = true,
                    FileExtension = ".mp4",
                    FileFilter = $"{Strings.Mp4File}|*.mp4",
                    MediaPathProvider = mediaPathProvider,
                    Converter = new PathConverter(parser, sectionName, "ThemeVideoLoopFile"),
                },
                new PathControlFactory()
                {
                    Name = THEME_VIDEO_STOP_FILE_CONTROL,
                    LabelText = Strings.ThemeVideoStopFile,
                    FileMode = true,
                    RelativeEnabled = true,
                    FileExtension = ".mp4",
                    FileFilter = $"{Strings.Mp4File}|*.mp4",
                    MediaPathProvider = mediaPathProvider,
                    Converter = new PathConverter(parser, sectionName, "ThemeVideoStopFile"),
                },
                new SelectControlFactory()
                {
                    Name = THEME_VIDEO_ROTATION_CONTROL,
                    LabelText = Strings.ThemeVideoRotation,
                    Items = new()
                    {
                        new("-90 °", -90),
                        new("0 °", 0),
                        new("90 °", 90),
                        new("180 °", 180),
                    },
                    Converter = new IntSelectConverter(parser, sectionName, "ThemeVideoRotation"),
                },
            ];
        }
    }
}
