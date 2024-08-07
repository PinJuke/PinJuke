﻿using PinJuke.Configuration;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PinJuke.Configurator.Factory
{
    public class GlobalGroupControlFactory : GroupControlFactory
    {
        public GlobalGroupControlFactory(Parser parser, PinUpPlayerIniReader pinUpReader)
        {
            // Some cases of Key share the same value.
            var keys = Enum.GetNames<Key>().Select(name => new Item(name, Enum.Parse<Key>(name))).ToList();

            LabelText = "Global configuration";
            Controls = [
                new GroupControlFactory()
                {
                    LabelText = "PinJuke",
                    Controls = [
                        new PathControlFactory()
                        {
                            LabelText = "Media path",
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "PinJuke", "MediaPath"),
                        },
                    ]
                },
                new WindowGroupControlFactory(parser, "PlayField", false, pinUpReader, "INFO3")
                {
                    LabelText = "Play field",
                },
                new WindowGroupControlFactory(parser, "BackGlass", false, pinUpReader, "INFO2")
                {
                    LabelText = "Back glass",
                },
                new WindowGroupControlFactory(parser, "DMD", true, pinUpReader, "INFO1")
                {
                    LabelText = "DMD",
                },
                new GroupControlFactory()
                {
                    LabelText = "Keyboard",
                    Controls = [
                        new SelectControlFactory()
                        {
                            LabelText = "Exit",
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Exit"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = "Browse",
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Browse"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = "Previous",
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Previous"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = "Next",
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Next"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = "Play/pause",
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "PlayPause"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = "Volume down",
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeDown"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = "Volume up",
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeUp"),
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = "Milkdrop",
                    Controls = [
                        new PathControlFactory()
                        {
                            LabelText = "Presets path",
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "Milkdrop", "PresetsPath"),
                        },
                        new PathControlFactory()
                        {
                            LabelText = "Textures",
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "Milkdrop", "TexturesPath"),
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = "DOF",
                    Controls = [
                        new BoolControlFactory()
                        {
                            LabelText = "Enabled",
                            Converter = new BoolConverter(parser, "DOF", "Enabled"),
                        },
                        new PathControlFactory()
                        {
                            LabelText = "Global config file path",
                            FileMode = true,
                            RelativeEnabled = false,
                            Converter = new PathConverter(parser, "DOF", "GlobalConfigFilePath"),
                        },
                    ]
                },
            ];
        }
    }

    public class WindowGroupControlFactory : GroupControlFactory
    {
        public WindowGroupControlFactory(Parser parser, string sectionName, bool enabledAvailable, PinUpPlayerIniReader pinUpReader, string pinUpSectionName)
        {
            ControlFactory<UIElement>[] enabledControls = enabledAvailable
                ? [
                    new BoolControlFactory()
                    {
                        LabelText = "Enabled",
                        Converter = new BoolConverter(parser, sectionName, "Enabled"),
                    },
                ]
                : [];

            Controls = [
                ..enabledControls,
                new ButtonControlFactory()
                {
                    LabelText = "",
                    Text = "Get display position from PinUP",
                    ClickHandler = (buttonControl) =>
                    {
                        (int, int, int, int)? position;
                        try
                        {
                            position = pinUpReader.FindPosition(pinUpSectionName);
                        }
                        catch (IniIoException ex)
                        {
                            MessageBox.Show(string.Format(Strings.ErrorReadingFile, ex.FilePath), AppDomain.CurrentDomain.FriendlyName);
                            return;
                        }
                        if (position == null)
                        {
                            MessageBox.Show(string.Format(Strings.PathNotFound, "PinUpPlayer.ini"), AppDomain.CurrentDomain.FriendlyName);
                            return;
                        }
                        ((NumberControl)buttonControl.GetSibling(1)).Value = position.Value.Item1;
                        ((NumberControl)buttonControl.GetSibling(2)).Value = position.Value.Item2;
                        ((NumberControl)buttonControl.GetSibling(3)).Value = position.Value.Item3;
                        ((NumberControl)buttonControl.GetSibling(4)).Value = position.Value.Item4;
                    },
                },
                new NumberControlFactory()
                {
                    LabelText = "Left",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowLeft"),
                },
                new NumberControlFactory()
                {
                    LabelText = "Top",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowTop"),
                },
                new NumberControlFactory()
                {
                    LabelText = "Width",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowWidth"),
                },
                new NumberControlFactory()
                {
                    LabelText = "Height",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowHeight"),
                },
                new NumberControlFactory()
                {
                    LabelText = "Scale",
                    Converter = new FloatNumberConverter(parser, sectionName, "ContentScale"),
                },
                new SelectControlFactory()
                {
                    LabelText = "Angle",
                    Items = new()
                    {
                        new("-90 °", -90),
                        new("0 °", 0),
                        new("90 °", 90),
                        new("180 °", 180),
                    },
                    Converter = new IntSelectConverter(parser, sectionName, "ContentAngle"),
                },
            ];
        }
    }
}
