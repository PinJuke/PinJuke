using PinJuke.Configurator.View;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static PinJuke.Configurator.View.ConfiguratorControl;
using static System.Resources.ResXFileRef;

namespace PinJuke.Configurator
{
    public interface ControlFactory<out T> where T : UIElement
    {
        public string? Name { get; set; }

        public T CreateControl();

        public void ReadFromControl(UIElement control, IniDocument iniDocument);
        public void WriteToControl(UIElement control, IniDocument iniDocument);
    }

    public class RowFactory<T> : ControlFactory<RowControl> where T : ConfiguratorControl
    {
        public string LabelText { get; set; } = "";
        public string? Name { get; set; } = null;

        private ControlFactory<T>? childFactory = null;
        public ControlFactory<T> ChildFactory
        {
            get
            {
                if (childFactory == null)
                {
                    throw new InvalidOperationException("ChildFactory is not set.");
                }
                return childFactory;
            }
            set
            {
                childFactory = value;
            }
        }

        public RowFactory()
        {
        }

        public RowControl CreateControl()
        {
            var rowControl = new RowControl();
            var childControl = ChildFactory.CreateControl();
            rowControl.Control = childControl;
            rowControl.LabelText = LabelText;
            return rowControl;
        }

        public virtual void ReadFromControl(UIElement control, IniDocument iniDocument)
        {
            var rowControl = (RowControl)control;
            var childControl = (T?)rowControl.Control ?? throw new InvalidOperationException("Cannot read. Control of row control is null.");
            ChildFactory.ReadFromControl(childControl, iniDocument);
        }

        public virtual void WriteToControl(UIElement control, IniDocument iniDocument)
        {
            var rowControl = (RowControl)control;
            var childControl = (T?)rowControl.Control ?? throw new InvalidOperationException("Cannot write. Control of row control is null.");
            ChildFactory.WriteToControl(childControl, iniDocument);
        }
    }

    public abstract class BaseControlFactory<T> : ControlFactory<T> where T : ConfiguratorControl
    {
        public string? Name { get; set; } = null;
        public string LabelText { get; set; } = "";
        public Converter<T>? Converter { get; set; } = null;
        public ConfiguratorControl.ChangedHandler? ChangedHandler { get; set; } = null;

        public abstract T CreateConfiguratorControl();

        public T CreateControl()
        {
            var control = CreateConfiguratorControl();
            if (ChangedHandler != null)
            {
                control.ChangedEvent += ChangedHandler;
            }
            return control;
        }

        public virtual void ReadFromControl(UIElement control, IniDocument iniDocument)
        {
            Converter?.ReadFromControl((T)control, iniDocument);
        }

        public virtual void WriteToControl(UIElement control, IniDocument iniDocument)
        {
            Converter?.WriteToControl((T)control, iniDocument);
        }
    }

    public interface ContainerControlFactoryInterface<out T> where T : ConfiguratorControl, ContainerControl
    {
        public ControlFactory<UIElement>[] Controls { get; }

        public T CreateContainerControl();
    }

    public class ContainerControlTrait<T> where T : ConfiguratorControl, ContainerControl
    {
        public T CreateControlAndChildren(ContainerControlFactoryInterface<T> factory)
        {
            var containerControl = factory.CreateContainerControl();
            foreach (var controlFactory in factory.Controls)
            {
                var control = controlFactory.CreateControl();
                containerControl.Controls.Children.Add(control);
            }
            return containerControl;
        }

        public void ReadFromControl(ContainerControlFactoryInterface<T> factory, UIElement control, IniDocument iniDocument)
        {
            var containerControl = (ContainerControl)control;
            var i = 0;
            foreach (var controlFactory in factory.Controls)
            {
                var childControl = containerControl.Controls.Children[i++];
                controlFactory.ReadFromControl(childControl, iniDocument);
            }
        }

        public void WriteToControl(ContainerControlFactoryInterface<T> factory, UIElement control, IniDocument iniDocument)
        {
            var containerControl = (ContainerControl)control;
            var i = 0;
            foreach (var controlFactory in factory.Controls)
            {
                var childControl = containerControl.Controls.Children[i++];
                controlFactory.WriteToControl(childControl, iniDocument);
            }
        }
    }

    public abstract class BaseContainerControlFactory<T> : BaseControlFactory<T>, ContainerControlFactoryInterface<T> where T : ConfiguratorControl, ContainerControl
    {
        public ControlFactory<UIElement>[] Controls { get; set; } = [];
        private readonly ContainerControlTrait<T> containerControlTrait = new();

        public abstract T CreateContainerControl();

        public override T CreateConfiguratorControl()
        {
            return containerControlTrait.CreateControlAndChildren(this);
        }

        public override void ReadFromControl(UIElement control, IniDocument iniDocument)
        {
            base.ReadFromControl(control, iniDocument);
            containerControlTrait.ReadFromControl(this, control, iniDocument);
        }

        public override void WriteToControl(UIElement control, IniDocument iniDocument)
        {
            base.WriteToControl(control, iniDocument);
            containerControlTrait.WriteToControl(this, control, iniDocument);
        }
    }

    public abstract class ContainerControlFactory<T> : ControlFactory<T>, ContainerControlFactoryInterface<T> where T : ConfiguratorControl, ContainerControl
    {
        public string LabelText { get; set; } = "";
        public string? Name { get; set; } = null;
        public ControlFactory<UIElement>[] Controls { get; set; } = [];
        private readonly ContainerControlTrait<T> containerControlTrait = new();

        public abstract T CreateContainerControl();

        public T CreateControl()
        {
            return containerControlTrait.CreateControlAndChildren(this);
        }

        public void ReadFromControl(UIElement control, IniDocument iniDocument)
        {
            containerControlTrait.ReadFromControl(this, control, iniDocument);
        }

        public void WriteToControl(UIElement control, IniDocument iniDocument)
        {
            containerControlTrait.WriteToControl(this, control, iniDocument);
        }
    }

    public class GroupControlFactory : ContainerControlFactory<GroupControl>
    {
        public override GroupControl CreateContainerControl()
        {
            return new GroupControl()
            {
                Name = Name,
                LabelText = LabelText,
            };
        }
    }

    public class PathControlFactory : BaseControlFactory<PathControl>
    {
        public bool EmptyEnabled { get; set; } = false;
        public bool RelativeEnabled { get; set; } = false;
        public bool RelativeDefault { get; set; } = true;
        public bool FileMode { get; set; } = false;
        public string FileExtension { get; set; } = ".ini";
        public string FileFilter { get; set; } = $"{Strings.IniFile}|*.ini";
        public MediaPathProvider? MediaPathProvider { get; set; } = null;
        public int InputWidth { get; set; } = 200;

        public override PathControl CreateConfiguratorControl()
        {
            return new PathControl()
            {
                Name = Name,
                EmptyEnabled = EmptyEnabled,
                RelativeDefault = RelativeDefault,
                RelativeEnabled = RelativeEnabled,
                FileMode = FileMode,
                FileExtension = FileExtension,
                FileFilter = FileFilter,
                MediaPathProvider = MediaPathProvider,
                InputWidth = InputWidth,
            };
        }
    }

    public class BoolControlFactory : BaseContainerControlFactory<BoolControl>
    {
        public override BoolControl CreateContainerControl()
        {
            return new BoolControl()
            {
                Name = Name,
            };
        }
    }

    public class NumberControlFactory : BaseControlFactory<NumberControl>
    {
        public override NumberControl CreateConfiguratorControl()
        {
            return new NumberControl()
            {
                Name = Name,
            };
        }
    }

    public class TextControlFactory : BaseControlFactory<TextControl>
    {
        public override TextControl CreateConfiguratorControl()
        {
            return new TextControl()
            {
                Name = Name,
            };
        }
    }

    public class SelectControlFactory : BaseControlFactory<SelectControl>
    {
        public List<Item> Items { get; set; } = new();

        public override SelectControl CreateConfiguratorControl()
        {
            return new SelectControl()
            {
                Name = Name,
                Items = Items
            };
        }
    }

    public class ButtonControlFactory : BaseControlFactory<ButtonControl>
    {
        public string Text { get; set; } = "";
        public ButtonControlClickHandler? ClickHandler { get; set; } = null;

        public override ButtonControl CreateConfiguratorControl()
        {
            return new ButtonControl()
            {
                Name = Name,
                Text = Text,
                ClickHandler = ClickHandler,
            };
        }
    }
}
