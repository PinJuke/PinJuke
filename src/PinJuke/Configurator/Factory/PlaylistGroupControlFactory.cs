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
                },
                new PathControlFactory()
                {
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
                },
                new PathControlFactory()
                {
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
