using PinJuke.Configurator.View;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PinJuke.Configurator
{
    public interface ControlFactory<out T> where T : UIElement
    {
        public string LabelText { get; set; }
        public string? Name { get; set; }

        public T CreateControl();

        public void ReadFromControl(UIElement control, IniDocument iniDocument);
        public void WriteToControl(UIElement control, IniDocument iniDocument);
    }

    public abstract class BaseControlFactory<T> : ControlFactory<RowControl> where T : ConfiguratorControl
    {
        public string LabelText { get; set; } = "";
        public string? Name { get; set; } = null;
        public Converter<T>? Converter { get; set; } = null;
        public ConfiguratorControl.ChangedHandler? ChangedHandler { get; set; } = null;

        public RowControl CreateControl()
        {
            var rowControl = new RowControl();
            var control = CreateControlForRow();
            if (ChangedHandler != null)
            {
                control.ChangedEvent += ChangedHandler;
            }
            rowControl.Control = control;
            rowControl.LabelText = LabelText;
            return rowControl;
        }

        public abstract T CreateControlForRow();

        public void ReadFromControl(UIElement control, IniDocument iniDocument)
        {
            var rowControl = (RowControl)control;
            var childControl = (T?)rowControl.Control ?? throw new InvalidOperationException("Cannot read. Control of row control is null.");
            Converter?.ReadFromControl(childControl, iniDocument);
        }

        public void WriteToControl(UIElement control, IniDocument iniDocument)
        {
            var rowControl = (RowControl)control;
            var childControl = (T?)rowControl.Control ?? throw new InvalidOperationException("Cannot write. Control of row control is null.");
            Converter?.WriteToControl(childControl, iniDocument);
        }
    }

    public class GroupControlFactory : ControlFactory<GroupControl>
    {
        public string LabelText { get; set; } = "";
        public string? Name { get; set; } = null;
        public ControlFactory<UIElement>[] Controls { get; set; } = [];

        public GroupControl CreateControl()
        {
            var controlGroup = new GroupControl()
            {
                Name = Name,
                LabelText = LabelText,
            };
            foreach (var controlFactory in Controls)
            {
                var control = controlFactory.CreateControl();
                controlGroup.Controls.Children.Add(control);
            }
            return controlGroup;
        }

        public void ReadFromControl(UIElement control, IniDocument iniDocument)
        {
            var groupControl = (GroupControl)control;
            foreach (var (controlFactory, i) in Controls.Select((item, i) => (item, i)))
            {
                var childControl = groupControl.Controls.Children[i];
                controlFactory.ReadFromControl(childControl, iniDocument);
            }
        }

        public void WriteToControl(UIElement control, IniDocument iniDocument)
        {
            var groupControl = (GroupControl)control;
            foreach (var (controlFactory, i) in Controls.Select((item, i) => (item, i)))
            {
                var childControl = groupControl.Controls.Children[i];
                controlFactory.WriteToControl(childControl, iniDocument);
            }
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

        public override PathControl CreateControlForRow()
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
            };
        }
    }

    public class BoolControlFactory : BaseControlFactory<BoolControl>
    {
        public override BoolControl CreateControlForRow()
        {
            return new BoolControl()
            {
                Name = Name,
            };
        }
    }

    public class NumberControlFactory : BaseControlFactory<NumberControl>
    {
        public override NumberControl CreateControlForRow()
        {
            return new NumberControl()
            {
                Name = Name,
            };
        }
    }

    public class SelectControlFactory : BaseControlFactory<SelectControl>
    {
        public List<Item> Items { get; set; } = new();

        public override SelectControl CreateControlForRow()
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

        public override ButtonControl CreateControlForRow()
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
