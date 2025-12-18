using PinJuke.Configuration;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using PinJuke.Utility;
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

            var descriptionMargin = new Thickness(10, 0, 0, 0);

            LabelText = Strings.GlobalConfiguration;
            Controls = [
                new GroupControlFactory()
                {
                    LabelText = "PinJuke",
                    Controls = [
                        new RowFactory() {
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
                new WindowGroupControlFactory(parser, "PlayField", false, pinUpReader, DisplayRole.PlayField)
                {
                    LabelText = Strings.DisplayPlayField,
                },
                new WindowGroupControlFactory(parser, "BackGlass", false, pinUpReader, DisplayRole.BackGlass)
                {
                    LabelText = Strings.DisplayBackGlass,
                },
                new WindowGroupControlFactory(parser, "DMD", true, pinUpReader, DisplayRole.DMD)
                {
                    LabelText = Strings.DisplayDmd,
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Input,
                    Controls = [
                        new TextBlockFactory()
                        {
                            Text = "⚠️ Controller input support (e.g. VirtuaPin) is coming soon. In the meantime, you can use tools like joy2key.",
                            Margin = new Thickness(0, 10, 0, 10),
                        },
                        new RowFactory() {
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new TextBlockFactory()
                                    {
                                        Text = Strings.Keyboard,
                                        Width = 200,
                                    },
                                ],
                            }
                        },
                        new RowFactory() {
                            LabelText = Strings.KeyExit,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new SelectControlFactory()
                                    {
                                        Items = keys,
                                        Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Exit"),
                                    },
                                    new TextBlockFactory()
                                    {
                                        Text = "(Exit)",
                                        Margin = descriptionMargin,
                                    }
                                ],
                            }
                        },
                        new RowFactory() {
                            LabelText = Strings.KeyBrowse,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new SelectControlFactory()
                                    {
                                        Items = keys,
                                        Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Browse"),
                                    },
                                    new TextBlockFactory()
                                    {
                                        Text = "(Launch ball)",
                                        Margin = descriptionMargin,
                                    }
                                ],
                            }
                        },
                        new RowFactory() {
                            LabelText = Strings.KeyPrevious,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new SelectControlFactory()
                                    {
                                        Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Previous"),
                                    },
                                    new TextBlockFactory()
                                    {
                                        Text = "(Left flipper)",
                                        Margin = descriptionMargin,
                                    }
                                ],
                            }
                        },
                        new RowFactory() {
                            LabelText = Strings.KeyNext,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new SelectControlFactory()
                                    {
                                        Items = keys,
                                        Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Next"),
                                    },
                                    new TextBlockFactory()
                                    {
                                        Text = "(Right flipper)",
                                        Margin = descriptionMargin,
                                    }
                                ],
                            }
                        },
                        new RowFactory() {
                            LabelText = Strings.KeyPlayPause,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new SelectControlFactory()
                                    {
                                        Items = keys,
                                        Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "PlayPause"),
                                    },
                                    new TextBlockFactory()
                                    {
                                        Text = "(Start)",
                                        Margin = descriptionMargin,
                                    }
                                ],
                            }
                        },
                        new RowFactory() {
                            LabelText = Strings.KeyVolumeDown,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new SelectControlFactory()
                                    {
                                        Items = keys,
                                        Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeDown"),
                                    },
                                    new TextBlockFactory()
                                    {
                                        Text = "(Left magna save)",
                                        Margin = descriptionMargin,
                                    }
                                ],
                            }
                        },
                        new RowFactory() {
                            LabelText = Strings.KeyVolumeUp,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new SelectControlFactory()
                                    {
                                        Items = keys,
                                        Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeUp"),
                                    },
                                    new TextBlockFactory()
                                    {
                                        Text = "(Right magna save)",
                                        Margin = descriptionMargin,
                                    }
                                ],
                            }
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Milkdrop,
                    Controls = [
                        new RowFactory() {
                            LabelText = Strings.MilkdropPresetsPath,
                            ChildFactory = new PathControlFactory()
                            {
                                FileMode = false,
                                RelativeEnabled = true,
                                Converter = new PathConverter(parser, "Milkdrop", "PresetsPath"),
                            }
                        },
                        new RowFactory() {
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
                        new RowFactory() {
                            LabelText = Strings.Enable,
                            ChildFactory = new BoolControlFactory()
                            {
                                Converter = new BoolConverter(parser, "DOF", "Enabled"),
                            }
                        },
                        new RowFactory() {
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
        public WindowGroupControlFactory(Parser parser, string sectionName, bool enabledAvailable, PinUpPlayerIniReader pinUpReader, DisplayRole displayRole)
        {
            ControlFactory<UIElement>[] enabledControls = enabledAvailable
                ? [
                    new RowFactory() {
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
                new RowFactory() {
                    LabelText = "",
                    ChildFactory = new ButtonControlFactory()
                    {
                        Text = Strings.GetDisplayPositionFromPinup,
                        ClickHandler = (buttonControl) =>
                        {
                            PinUpRect? rect = null;
                            try
                            {
                                rect = pinUpReader.FindPosition(displayRole);
                            }
                            catch (IniIoException ex)
                            {
                                UiUtil.ShowErrorMessage(string.Format(Strings.ErrorReadingFile, ex.FilePath));
                                return;
                            }
                            if (rect == null)
                            {
                                UiUtil.ShowErrorMessage(string.Format(Strings.PathNotFound, PinUpPlayerIniReader.BALLER_PIN_UP_PLAYER_INI));
                                return;
                            }
                            var group = buttonControl.GetParentGroup();
                            ((NumberControl)group.GetChildByName("WindowLeft")).Value = rect.Left;
                            ((NumberControl)group.GetChildByName("WindowTop")).Value = rect.Top;
                            ((NumberControl)group.GetChildByName("WindowWidth")).Value = rect.Width;
                            ((NumberControl)group.GetChildByName("WindowHeight")).Value = rect.Height;
                        },
                    }
                },
                new RowFactory() {
                    LabelText = Strings.RectLeft,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowLeft",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowLeft"),
                    }
                },
                new RowFactory() {
                    LabelText = Strings.RectTop,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowTop",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowTop"),
                    }
                },
                new RowFactory() {
                    LabelText = Strings.RectWidth,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowWidth",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowWidth"),
                    }
                },
                new RowFactory() {
                    LabelText = Strings.RectHeight,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowHeight",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowHeight"),
                    }
                },
                new RowFactory() {
                    LabelText = Strings.Scale,
                    ChildFactory = new NumberControlFactory()
                    {
                        Converter = new FloatNumberConverter(parser, sectionName, "ContentScale"),
                    }
                },
                new RowFactory() {
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
