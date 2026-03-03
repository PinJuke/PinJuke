using PinJuke.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke.Configurator.View
{
    public class ConfiguratorControl : BaseControl
    {
        public delegate void ChangedHandler(ConfiguratorControl control);

        public event ChangedHandler? ChangedEvent;

        public ConfiguratorControl()
        {
        }

        public static T? FindControlParent<T>(FrameworkElement control) where T : FrameworkElement
        {
            FrameworkElement? element = control.Parent as FrameworkElement;
            for (; ; )
            {
                if (element == null || element is T)
                {
                    return (T?)element;
                }
                element = element.Parent as FrameworkElement;
            }
        }

        public static T GetControlParent<T>(FrameworkElement control) where T : FrameworkElement
        {
            var parent = FindControlParent<T>(control);
            if (parent == null)
            {
                throw new InvalidOperationException("Cannot return parent. No parent found.");
            }
            return parent;
        }

        public T? FindParent<T>() where T : FrameworkElement
        {
            return FindControlParent<T>(this);
        }

        public T GetParent<T>() where T : FrameworkElement
        {
            return GetControlParent<T>(this);
        }

        protected void OnChanged()
        {
            ChangedEvent?.Invoke(this);
            var parentGroup = FindParent<ContainerControl>();
            parentGroup?.OnChanged();
        }
    }
}
