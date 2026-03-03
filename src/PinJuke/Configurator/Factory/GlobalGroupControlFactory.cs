using PinJuke.Configurator.View;
using PinJuke.Ini;
using PinJuke.Service;
using PinJuke.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PinJuke.Configurator.Factory
{
    public class GlobalGroupControlFactory : GroupControlFactory
    {
        public const string MEDIA_PATH_CONTROL = "MediaPath";

        public const string KEYBOARD_CONTROL = "Keyboard";
        public const string CONTROLLER_CONTROL = "Controller";

        private readonly Configuration.Parser parser;
        private readonly List<Item> keys;
        private readonly List<Item> controllerButtons;
        private readonly Thickness inputMargin;
        private readonly ButtonControlFactory keyboardCaptureFactory;
        private readonly ButtonControlFactory gamepadCaptureFactory;


        public GlobalGroupControlFactory(Configuration.Parser parser, PinUpPlayerIniReader pinUpReader)
        {
            this.parser = parser;

            // Some cases of Key share the same value.
            keys = Enum.GetNames<Key>().Select(name => new Item(name, Enum.Parse<Key>(name))).ToList();

            // Create controller button options (1-32 for typical gamepad controllers)
            controllerButtons = new List<Item>();
            controllerButtons.Add(new Item("None", Configuration.ControllerButton.NoButton)); // Option for no button assigned
            for (int i = 1; i <= 32; i++)
            {
                controllerButtons.Add(new Item($"Button {i}", i));
            }

            inputMargin = new Thickness(10, 0, 0, 0);
            keyboardCaptureFactory = new ButtonControlFactory()
            {
                Text = "⌨️",
                ClickHandler = OnCaptureKeyboardClicked,
                Width = 20,
            };
            gamepadCaptureFactory = new ButtonControlFactory()
            {
                Text = "🎮",
                ClickHandler = OnCaptureGamepadClicked,
                Width = 20,
            };

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
                new WindowGroupControlFactory(parser, "PlayField", false, pinUpReader, Configuration.DisplayRole.PlayField)
                {
                    LabelText = Strings.DisplayPlayField,
                },
                new WindowGroupControlFactory(parser, "BackGlass", false, pinUpReader, Configuration.DisplayRole.BackGlass)
                {
                    LabelText = Strings.DisplayBackGlass,
                },
                new WindowGroupControlFactory(parser, "DMD", true, pinUpReader, Configuration.DisplayRole.DMD)
                {
                    LabelText = Strings.DisplayDmd,
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Input,
                    Controls = [
                        new RowFactory() {
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = [
                                    new TextBlockFactory()
                                    {
                                        Text = Strings.Keyboard,
                                        Width = 200,
                                    },
                                    new TextBlockFactory()
                                    {
                                        Inline = () => {
                                            var span = new Span();
                                            span.Inlines.Add(new Run(Strings.Controller));
                                            span.Inlines.Add(" ");
                                            var link = new Hyperlink(new Run(Strings.Presets));
                                            link.Click += SelectFromGamepadPresets;
                                            span.Inlines.Add(link);
                                            return span;
                                        },
                                        Width = 200,
                                        Margin = inputMargin,
                                    },
                                ],
                            }
                        },
                        new RowFactory() {
                            Name = "Exit",
                            LabelText = Strings.KeyExit,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = CreateInputRow("Exit", "(Exit)"),
                            }
                        },
                        new RowFactory() {
                            Name = "Browse",
                            LabelText = Strings.KeyBrowse,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = CreateInputRow("Browse", "(Launch ball)"),
                            }
                        },
                        new RowFactory() {
                            Name = "Previous",
                            LabelText = Strings.KeyPrevious,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = CreateInputRow("Previous", "(Left flipper)"),
                            }
                        },
                        new RowFactory() {
                            Name = "Next",
                            LabelText = Strings.KeyNext,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = CreateInputRow("Next", "(Right flipper)"),
                            }
                        },
                        new RowFactory() {
                            Name = "PlayPause",
                            LabelText = Strings.KeyPlayPause,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = CreateInputRow("PlayPause", "(Start)"),
                            }
                        },
                        new RowFactory() {
                            Name = "VolumeDown",
                            LabelText = Strings.KeyVolumeDown,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = CreateInputRow("VolumeDown", "(Left magna save)"),
                            }
                        },
                        new RowFactory() {
                            Name = "VolumeUp",
                            LabelText = Strings.KeyVolumeUp,
                            ChildFactory = new CompositeControlFactory()
                            {
                                Children = CreateInputRow("VolumeUp", "(Right magna save)"),
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

        private ControlFactory<UIElement>[] CreateInputRow(string entryName, string description)
        {
            return [
                new SelectControlFactory()
                {
                    Name = KEYBOARD_CONTROL,
                    Items = keys,
                    Converter = new EnumSelectConverter<Key>(parser, "Keyboard", entryName),
                    Width = 180,
                },
                keyboardCaptureFactory,
                new SelectControlFactory()
                {
                    Name = CONTROLLER_CONTROL,
                    Items = controllerButtons,
                    Converter = new IntSelectConverter(parser, "Controller", entryName, Configuration.ControllerButton.NoButton),
                    Margin = inputMargin,
                    Width = 180,
                },
                gamepadCaptureFactory,
                new TextBlockFactory()
                {
                    Text = description,
                    Margin = inputMargin,
                }
            ];
        }

        private void SelectFromGamepadPresets(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var hyperlink = (Hyperlink)sender;
            var span = (Span)hyperlink.Parent;
            var control = (FrameworkElement)span.Parent;
            var group = ConfiguratorControl.GetControlParent<GroupControl>(control);

            var window = new PresetWindow();
            window.Owner = Window.GetWindow(group);
            window.Presets.Items.Add(new Item("VirtuaPin", "VirtuaPin"));
            window.Selected += (sender, e) => ApplyGamepadPresets(group, ((string?)((Item?)e.SelectedItem)?.Value) ?? "");
            window.ShowDialog();
        }

        private void ApplyGamepadPresets(ContainerControl group, string type)
        {
            switch (type)
            {
                case "VirtuaPin":
                    ApplyGamepadPreset(group, "Exit", 12);
                    ApplyGamepadPreset(group, "Browse", 0);
                    ApplyGamepadPreset(group, "Previous", 10);
                    ApplyGamepadPreset(group, "Next", 2);
                    ApplyGamepadPreset(group, "PlayPause", 9);
                    ApplyGamepadPreset(group, "VolumeDown", 11);
                    ApplyGamepadPreset(group, "VolumeUp", 3);
                    break;
            }
        }

        private void ApplyGamepadPreset(ContainerControl group, string type, int value)
        {
            var row = (RowControl)group.GetChildByName(type);
            var select = (SelectControl)row.GetChildByName(CONTROLLER_CONTROL);
            select.SelectedValue = value;
        }

        private void OnCaptureKeyboardClicked(ConfiguratorControl control)
        {
            var group = control.GetParent<RowControl>();
            var select = (SelectControl)group.GetChildByName(KEYBOARD_CONTROL);
            var window = new KeyboardCaptureWindow();
            window.Owner = Window.GetWindow(select);
            window.KeyPressed += (sender, e) => select.SelectedValue = e.Key;
            window.ShowDialog();
        }

        private void OnCaptureGamepadClicked(ConfiguratorControl control)
        {
            var group = control.GetParent<RowControl>();
            var select = (SelectControl)group.GetChildByName(CONTROLLER_CONTROL);
            var window = new ControllerCaptureWindow();
            window.Owner = Window.GetWindow(select);
            window.ButtonPressed += (sender, e) => select.SelectedValue = e.ButtonNumber;
            window.ShowDialog();
        }
    }

    public class WindowGroupControlFactory : GroupControlFactory
    {
        public WindowGroupControlFactory(Configuration.Parser parser, string sectionName, bool enabledAvailable, PinUpPlayerIniReader pinUpReader, Configuration.DisplayRole displayRole)
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
                            var group = buttonControl.GetParent<GroupControl>();
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
