using PinJuke.Configuration;
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
        public const string MEDIA_PATH_CONTROL = "MediaPath";

        public GlobalGroupControlFactory(Parser parser, PinUpPlayerIniReader pinUpReader)
        {
            // Some cases of Key share the same value.
            var keys = Enum.GetNames<Key>().Select(name => new Item(name, Enum.Parse<Key>(name))).ToList();

            LabelText = Strings.GlobalConfiguration;
            Controls = [
                new GroupControlFactory()
                {
                    LabelText = "PinJuke",
                    Controls = [
                        new PathControlFactory()
                        {
                            LabelText = Strings.MediaPath,
                            Name = MEDIA_PATH_CONTROL,
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "PinJuke", "MediaPath"),
                        },
                    ]
                },
                new WindowGroupControlFactory(parser, "PlayField", false, pinUpReader, PinUpPlayerIniReader.PLAY_FIELD_SECTION_NAME)
                {
                    LabelText = Strings.DisplayPlayField,
                },
                new WindowGroupControlFactory(parser, "BackGlass", false, pinUpReader, PinUpPlayerIniReader.BACK_GLASS_SECTION_NAME)
                {
                    LabelText = Strings.DisplayBackGlass,
                },
                new WindowGroupControlFactory(parser, "DMD", true, pinUpReader, PinUpPlayerIniReader.DMD_SECTION_NAME)
                {
                    LabelText = Strings.DisplayDmd,
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Keyboard,
                    Controls = [
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyExit,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Exit"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyBrowse,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Browse"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyPrevious,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Previous"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyNext,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Next"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyPlayPause,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "PlayPause"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyVolumeDown,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeDown"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyVolumeUp,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeUp"),
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Milkdrop,
                    Controls = [
                        new PathControlFactory()
                        {
                            LabelText = Strings.MilkdropPresetsPath,
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "Milkdrop", "PresetsPath"),
                        },
                        new PathControlFactory()
                        {
                            LabelText = Strings.MilkdropTexturesPath,
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "Milkdrop", "TexturesPath"),
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Dof,
                    Controls = [
                        new BoolControlFactory()
                        {
                            LabelText = Strings.Enable,
                            Converter = new BoolConverter(parser, "DOF", "Enabled"),
                        },
                        new PathControlFactory()
                        {
                            LabelText = Strings.DofGlobalConfigFilePath,
                            FileMode = true,
                            RelativeEnabled = false,
                            FileExtension = ".xml",
                            FileFilter = $"{Strings.XmlFile}|*.xml",
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
                        LabelText = Strings.Enable,
                        Converter = new BoolConverter(parser, sectionName, "Enabled"),
                    },
                ]
                : [];

            Controls = [
                ..enabledControls,
                new ButtonControlFactory()
                {
                    LabelText = "",
                    Text = Strings.GetDisplayPositionFromPinup,
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
                            MessageBox.Show(string.Format(Strings.PathNotFound, PinUpPlayerIniReader.BALLER_PIN_UP_PLAYER_INI), AppDomain.CurrentDomain.FriendlyName);
                            return;
                        }
                        var group = buttonControl.GetParentGroup();
                        ((NumberControl)group.GetChildByName("WindowLeft")).Value = position.Value.Item1;
                        ((NumberControl)group.GetChildByName("WindowTop")).Value = position.Value.Item2;
                        ((NumberControl)group.GetChildByName("WindowWidth")).Value = position.Value.Item3;
                        ((NumberControl)group.GetChildByName("WindowHeight")).Value = position.Value.Item4;
                    },
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.RectLeft,
                    Name = "WindowLeft",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowLeft"),
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.RectTop,
                    Name = "WindowTop",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowTop"),
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.RectWidth,
                    Name = "WindowWidth",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowWidth"),
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.RectHeight,
                    Name = "WindowHeight",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowHeight"),
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.Scale,
                    Converter = new FloatNumberConverter(parser, sectionName, "ContentScale"),
                },
                new SelectControlFactory()
                {
                    LabelText = Strings.Rotation,
                    Items = new()
                    {
                        new("-90 °", -90),
                        new("0 °", 0),
                        new("90 °", 90),
                        new("180 °", 180),
                    },
                    Converter = new IntSelectConverter(parser, sectionName, "ContentRotation"),
                },
            ];
        }
    }
}
