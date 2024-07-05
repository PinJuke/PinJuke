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
        public PlaylistGroupControlFactory(Parser parser)
        {
            LabelText = "Playlist configuration";
            Controls = [
                new GroupControlFactory()
                {
                    LabelText = "Player",
                    Controls = [
                        new PathControlFactory()
                        {
                            LabelText = "Music path",
                            FileMode = false,
                            RelativeEnabled = false,
                            Converter = new PathConverter(parser, "Player", "MusicPath"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = "Startup track type",
                            Items = new()
                            {
                                new("Last played track", 0),
                                new("First track in root folder", 1),
                                new("Random mode", 2),
                            },
                            Converter = new IntSelectConverter(parser, "Player", "StartupTrackType"),
                        },
                        new BoolControlFactory()
                        {
                            LabelText = "Play on startup",
                            Converter = new BoolConverter(parser, "Player", "PlayOnStartup"),
                        },
                    ]
                },
                new ContentGroupControlFactory(parser, "PlayField")
                {
                    LabelText = "Play field",
                },
                new ContentGroupControlFactory(parser, "BackGlass")
                {
                    LabelText = "Back glass",
                },
                new ContentGroupControlFactory(parser, "DMD")
                {
                    LabelText = "DMD",
                },
            ];
        }
    }

    public class ContentGroupControlFactory : GroupControlFactory
    {
        public ContentGroupControlFactory(Parser parser, string sectionName)
        {
            Controls = [
                new SelectControlFactory()
                {
                    LabelText = "Background type",
                    Items = new()
                    {
                        new("Show specified image", 0),
                        new("Show milkdrop visualizations", 1),
                    },
                    Converter = new IntSelectConverter(parser, sectionName, "BackgroundType"),
                },
                new PathControlFactory()
                {
                    LabelText = "Background image file",
                    FileMode = true,
                    RelativeEnabled = true,
                    Converter = new PathConverter(parser, sectionName, "BackgroundImageFile"),
                },
                new BoolControlFactory()
                {
                    LabelText = "Cover enabled",
                    Converter = new BoolConverter(parser, sectionName, "CoverEnabled"),
                },
                new BoolControlFactory()
                {
                    LabelText = "State enabled",
                    Converter = new BoolConverter(parser, sectionName, "StateEnabled"),
                },
                new BoolControlFactory()
                {
                    LabelText = "Browser enabled",
                    Converter = new BoolConverter(parser, sectionName, "BrowserEnabled"),
                },
                new PathControlFactory()
                {
                    LabelText = "Song start file",
                    FileMode = true,
                    RelativeEnabled = true,
                    Converter = new PathConverter(parser, sectionName, "SongStartFile"),
                },
            ];
        }
    }
}
