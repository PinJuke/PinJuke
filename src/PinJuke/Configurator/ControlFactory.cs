﻿using PinJuke.Configurator.View;
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

        public T CreateControl();

        public void ReadFromControl(UIElement control, IniDocument iniDocument);
        public void WriteToControl(UIElement control, IniDocument iniDocument);
    }

    public abstract class BaseControlFactory<T> : ControlFactory<RowControl> where T : UIElement
    {
        public string LabelText { get; set; } = "";
        public Converter<T>? Converter { get; set; } = null;

        public RowControl CreateControl()
        {
            var rowControl = new RowControl();
            var control = CreateControlForRow();
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
        public ControlFactory<UIElement>[] Controls { get; set; } = [];

        public GroupControl CreateControl()
        {
            var controlGroup = new GroupControl() { LabelText = LabelText };
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
        public bool FileMode { get; set; } = false;
        public string FileExtension { get; set; } = ".ini";
        public string FileFilter { get; set; } = "Ini file|*.ini";

        public override PathControl CreateControlForRow()
        {
            return new PathControl()
            {
                EmptyEnabled = EmptyEnabled,
                RelativeEnabled = RelativeEnabled,
                FileMode = FileMode,
                FileExtension = FileExtension,
                FileFilter = FileFilter,
            };
        }
    }

    public class BoolControlFactory : BaseControlFactory<BoolControl>
    {
        public override BoolControl CreateControlForRow()
        {
            return new BoolControl();
        }
    }

    public class NumberControlFactory : BaseControlFactory<NumberControl>
    {
        public override NumberControl CreateControlForRow()
        {
            return new NumberControl();
        }
    }

    public class SelectControlFactory : BaseControlFactory<SelectControl>
    {
        public List<Item> Items { get; set; } = new();

        public override SelectControl CreateControlForRow()
        {
            return new SelectControl() { Items = Items };
        }
    }

    public class ButtonControlFactory : BaseControlFactory<ButtonControl>
    {
        public string Text { get; set; } = "";
        public ButtonControlClickHandler? ClickHandler { get; set; } = null;

        public override ButtonControl CreateControlForRow()
        {
            return new ButtonControl() { Text = Text, ClickHandler = ClickHandler, };
        }
    }
}
