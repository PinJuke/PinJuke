using PinJuke.Configuration;
using PinJuke.Configurator.View;
using PinJuke.Ini;
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
                        new RowFactory<PathControl>() {
                            LabelText = Strings.MusicPath,
                            ChildFactory = new PathControlFactory()
                            {
                                FileMode = false,
                                RelativeEnabled = false,
                                Converter = new PathConverter(parser, "Player", "MusicPath"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.StartupTrackType,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = new()
                                {
                                    new(Strings.StartupTrackTypeLastPlayedTrack, 0),
                                    new(Strings.StartupTrackTypeFirstTrack, 1),
                                    new(Strings.StartupTrackTypeRandomMode, 2),
                                },
                                Converter = new IntSelectConverter(parser, "Player", "StartupTrackType"),
                            }
                        },
                        new RowFactory<BoolControl>() {
                            LabelText = Strings.PlayOnStartup,
                            ChildFactory = new BoolControlFactory()
                            {
                                Converter = new BoolConverter(parser, "Player", "PlayOnStartup"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.TrackBrowserOn,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = new()
                                {
                                    new(Strings.DisplayPlayField, 0),
                                    new(Strings.DisplayBackGlass, 1),
                                    new(Strings.DisplayDmd, 2),
                                },
                                Converter = new TrackBrowserOnConverter(parser),
                            }
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

    public class TrackBrowserOnConverter : Converter<SelectControl>
    {
        public Parser Parser { get; }

        public TrackBrowserOnConverter(Parser parser)
        {
            Parser = parser;
        }

        public void ReadFromControl(SelectControl control, IniDocument iniDocument)
        {
            var selectedValue = (int?)control.SelectedValue;
            iniDocument["PlayField"]["BrowserEnabled"] = Parser.FormatBool(selectedValue == 0);
            iniDocument["BackGlass"]["BrowserEnabled"] = Parser.FormatBool(selectedValue == 1);
            iniDocument["DMD"]["BrowserEnabled"] = Parser.FormatBool(selectedValue == 2);
        }

        public void WriteToControl(SelectControl control, IniDocument iniDocument)
        {
            int? selectedValue = null;
            if (Parser.ParseBool(iniDocument["PlayField"]["BrowserEnabled"]) == true)
            {
                selectedValue = 0;
            }
            else if (Parser.ParseBool(iniDocument["BackGlass"]["BrowserEnabled"]) == true)
            {
                selectedValue = 1;
            }
            else if (Parser.ParseBool(iniDocument["DMD"]["BrowserEnabled"]) == true)
            {
                selectedValue = 2;
            }
            control.SelectedValue = selectedValue;
        }
    }

    public class ContentGroupControlFactory : GroupControlFactory
    {
        public const string PLAYBACK_BACKGROUND_TYPE_CONTROL = "PlaybackBackgroundType";
        public const string IDLE_BACKGROUND_TYPE_CONTROL = "IdleBackgroundType";
        public const string BACKGROUND_IMAGE_FILE_CONTROL = "BackgroundImageFile";
        public const string THEME_VIDEO_START_FILE_CONTROL = "ThemeVideoStartFile";
        public const string THEME_VIDEO_LOOP_FILE_CONTROL = "ThemeVideoLoopFile";
        public const string THEME_VIDEO_STOP_FILE_CONTROL = "ThemeVideoStopFile";
        public const string THEME_VIDEO_IDLE_FILE_CONTROL = "ThemeVideoIdleFile";
        public const string THEME_VIDEO_ROTATION_CONTROL = "ThemeVideoRotation";

        public ContentGroupControlFactory(Parser parser, string sectionName, MediaPathProvider mediaPathProvider)
        {
            Controls = [
                new RowFactory<BoolControl>() {
                    LabelText = Strings.EnableTrackCover,
                    ChildFactory = new BoolControlFactory()
                    {
                        Converter = new BoolConverter(parser, sectionName, "CoverEnabled"),
                    }
                },

                new RowFactory<SelectControl>() {
                    LabelText = Strings.PlaybackBackgroundType,
                    ChildFactory = new SelectControlFactory()
                    {
                        Name = PLAYBACK_BACKGROUND_TYPE_CONTROL,
                        Items = new()
                        {
                            new(Strings.BackgroundTypeShowSpecifiedImage, 0),
                            new(Strings.BackgroundTypeShowMilkdropVisualizations, 1),
                            new(Strings.BackgroundTypeShowLoopVideo, 2),
                        },
                        Converter = new IntSelectConverter(parser, sectionName, "PlaybackBackgroundType"),
                        ChangedHandler = OnBackgroundTypeChanged,
                    }
                },
                new RowFactory<SelectControl>() {
                    LabelText = Strings.IdleBackgroundType,
                    ChildFactory = new SelectControlFactory()
                    {
                        Name = IDLE_BACKGROUND_TYPE_CONTROL,
                        Items = new()
                        {
                            new(Strings.BackgroundTypeShowSpecifiedImage, 0),
                            new(Strings.BackgroundTypeShowMilkdropVisualizations, 1),
                            new(Strings.BackgroundTypeShowIdleVideo, 2),
                        },
                        Converter = new IntSelectConverter(parser, sectionName, "IdleBackgroundType"),
                        ChangedHandler = OnBackgroundTypeChanged,
                    }
                },

                new RowFactory<PathControl>() {
                    LabelText = Strings.BackgroundImageFile,
                    ChildFactory = new PathControlFactory()
                    {
                        Name = BACKGROUND_IMAGE_FILE_CONTROL,
                        FileMode = true,
                        RelativeEnabled = true,
                        FileExtension = ".jpg",
                        FileFilter = $"{Strings.JpegFile}|*.jpg;*.jpeg;*.jpe;*.jfif",
                        MediaPathProvider = mediaPathProvider,
                        Converter = new PathConverter(parser, sectionName, "BackgroundImageFile"),
                    }
                },

                new RowFactory<PathControl>() {
                    LabelText = Strings.ThemeVideoLoopFile,
                    ChildFactory = new PathControlFactory()
                    {
                        Name = THEME_VIDEO_LOOP_FILE_CONTROL,
                        FileMode = true,
                        RelativeEnabled = true,
                        FileExtension = ".mp4",
                        FileFilter = $"{Strings.Mp4File}|*.mp4",
                        MediaPathProvider = mediaPathProvider,
                        Converter = new PathConverter(parser, sectionName, "ThemeVideoLoopFile"),
                    }
                },
                new RowFactory<PathControl>() {
                    LabelText = Strings.ThemeVideoIdleFile,
                    ChildFactory = new PathControlFactory()
                    {
                        Name = THEME_VIDEO_IDLE_FILE_CONTROL,
                        FileMode = true,
                        RelativeEnabled = true,
                        FileExtension = ".mp4",
                        FileFilter = $"{Strings.Mp4File}|*.mp4",
                        MediaPathProvider = mediaPathProvider,
                        Converter = new PathConverter(parser, sectionName, "ThemeVideoIdleFile"),
                    }
                },

                new RowFactory<BoolControl>() {
                    LabelText = Strings.ThemeVideoStartFile,
                    ChildFactory = new BoolControlFactory()
                    {
                        Converter = new BoolConverter(parser, sectionName, "ThemeVideoStartFileEnabled"),
                        Controls = [
                            new PathControlFactory()
                            {
                                Name = THEME_VIDEO_START_FILE_CONTROL,
                                FileMode = true,
                                RelativeEnabled = true,
                                FileExtension = ".mp4",
                                FileFilter = $"{Strings.Mp4File}|*.mp4",
                                MediaPathProvider = mediaPathProvider,
                                Converter = new PathConverter(parser, sectionName, "ThemeVideoStartFile"),
                                InputWidth = 180,
                            },
                        ],
                    }
                },
                new RowFactory<BoolControl>() {
                    LabelText = Strings.ThemeVideoStopFile,
                    ChildFactory = new BoolControlFactory()
                    {
                        Converter = new BoolConverter(parser, sectionName, "ThemeVideoStopFileEnabled"),
                        Controls = [
                            new PathControlFactory()
                            {
                                Name = THEME_VIDEO_STOP_FILE_CONTROL,
                                FileMode = true,
                                RelativeEnabled = true,
                                FileExtension = ".mp4",
                                FileFilter = $"{Strings.Mp4File}|*.mp4",
                                MediaPathProvider = mediaPathProvider,
                                Converter = new PathConverter(parser, sectionName, "ThemeVideoStopFile"),
                                InputWidth = 180,
                            },
                        ],
                    }
                },


                new RowFactory<SelectControl>() {
                    LabelText = Strings.ThemeVideoRotation,
                    ChildFactory = new SelectControlFactory()
                    {
                        Name = THEME_VIDEO_ROTATION_CONTROL,
                        Items = new()
                        {
                            new("-90 °", -90),
                            new("0 °", 0),
                            new("90 °", 90),
                            new("180 °", 180),
                        },
                        Converter = new IntSelectConverter(parser, sectionName, "ThemeVideoRotation"),
                    }
                },
            ];
        }

        private void OnBackgroundTypeChanged(ConfiguratorControl control)
        {
            var group = control.GetParentGroup();

            var playbackBackgroundTypeControl = (SelectControl)group.GetChildByName(PLAYBACK_BACKGROUND_TYPE_CONTROL);
            var idleBackgroundTypeControl = (SelectControl)group.GetChildByName(IDLE_BACKGROUND_TYPE_CONTROL);

            var backgroundImageFileControl = (PathControl)group.GetChildByName(BACKGROUND_IMAGE_FILE_CONTROL);
            var themeVideoLoopFileControl = (PathControl)group.GetChildByName(THEME_VIDEO_LOOP_FILE_CONTROL);
            var themeVideoIdleFileControl = (PathControl)group.GetChildByName(THEME_VIDEO_IDLE_FILE_CONTROL);

            {
                backgroundImageFileControl.Enabled =
                    (playbackBackgroundTypeControl.SelectedValue is int playbackType && playbackType == 0)
                    || (idleBackgroundTypeControl.SelectedValue is int idleType && idleType == 0);
            }
            {
                themeVideoLoopFileControl.Enabled = playbackBackgroundTypeControl.SelectedValue is int playbackType && playbackType == 2;
                themeVideoIdleFileControl.Enabled = idleBackgroundTypeControl.SelectedValue is int idleType && idleType == 2;
            }
        }
    }
}
