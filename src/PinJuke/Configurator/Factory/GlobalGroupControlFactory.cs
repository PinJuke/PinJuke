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
                        new RowFactory<PathControl>() {
                            LabelText = Strings.MediaPath,
                            ChildFactory = new PathControlFactory()
                            {
                                Name = MEDIA_PATH_CONTROL,
                                FileMode = false,
                                RelativeEnabled = true,
                                Converter = new PathConverter(parser, "PinJuke", "MediaPath"),
                            }
                        }
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
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyExit,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Exit"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyBrowse,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Browse"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyPrevious,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Previous"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyNext,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Next"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyPlayPause,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "PlayPause"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyVolumeDown,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeDown"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyVolumeUp,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeUp"),
                            }
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Milkdrop,
                    Controls = [
                        new RowFactory<PathControl>() {
                            LabelText = Strings.MilkdropPresetsPath,
                            ChildFactory = new PathControlFactory()
                            {
                                FileMode = false,
                                RelativeEnabled = true,
                                Converter = new PathConverter(parser, "Milkdrop", "PresetsPath"),
                            }
                        },
                        new RowFactory<PathControl>() {
                            LabelText = Strings.MilkdropTexturesPath,
                            ChildFactory = new PathControlFactory()
                            {
                                FileMode = false,
                                RelativeEnabled = true,
                                Converter = new PathConverter(parser, "Milkdrop", "TexturesPath"),
                            }
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Dof,
                    Controls = [
                        new RowFactory<BoolControl>() {
                            LabelText = Strings.Enable,
                            ChildFactory = new BoolControlFactory()
                            {
                                Converter = new BoolConverter(parser, "DOF", "Enabled"),
                            }
                        },
                        new RowFactory<PathControl>() {
                            LabelText = Strings.DofGlobalConfigFilePath,
                            ChildFactory = new PathControlFactory()
                            {
                                FileMode = true,
                                RelativeEnabled = false,
                                FileExtension = ".xml",
                                FileFilter = $"{Strings.XmlFile}|*.xml",
                                Converter = new PathConverter(parser, "DOF", "GlobalConfigFilePath"),
                            }
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
                    new RowFactory<BoolControl>() {
                        LabelText = Strings.Enable,
                        ChildFactory = new BoolControlFactory()
                        {
                            Converter = new BoolConverter(parser, sectionName, "Enabled"),
                        }
                    },
                ]
                : [];

            Controls = [
                ..enabledControls,
                new RowFactory<ButtonControl>() {
                    LabelText = "",
                    ChildFactory = new ButtonControlFactory()
                    {
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
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.RectLeft,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowLeft",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowLeft"),
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.RectTop,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowTop",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowTop"),
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.RectWidth,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowWidth",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowWidth"),
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.RectHeight,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowHeight",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowHeight"),
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.Scale,
                    ChildFactory = new NumberControlFactory()
                    {
                        Converter = new FloatNumberConverter(parser, sectionName, "ContentScale"),
                    }
                },
                new RowFactory<SelectControl>() {
                    LabelText = Strings.Rotation,
                    ChildFactory = new SelectControlFactory()
                    {
                        Items = new()
                        {
                            new("-90 °", -90),
                            new("0 °", 0),
                            new("90 °", 90),
                            new("180 °", 180),
                        },
                        Converter = new IntSelectConverter(parser, sectionName, "ContentRotation"),
                    }
                },
            ];
        }
    }
}
