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
        public ConfiguratorControl()
        {
        }

        private T? FindParent<T>() where T : FrameworkElement
        {
            FrameworkElement? element = Parent as FrameworkElement;
            for (; ; )
            {
                if (element == null || element is T)
                {
                    return (T?)element;
                }
                element = element.Parent as FrameworkElement;
            }
        }

        public GroupControl? FindParentGroup()
        {
            return FindParent<GroupControl>();
        }

        public GroupControl GetParentGroup()
        {
            var parentGroup = FindParentGroup();
            if (parentGroup == null)
            {
                throw new InvalidOperationException("Cannot return parent group. No parent group found.");
            }
            return parentGroup;
        }
    }
}
