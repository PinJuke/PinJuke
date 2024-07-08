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

        public ConfiguratorControl GetSibling(int n)
        {
            var rowControl = FindParent<RowControl>();
            if (rowControl == null)
            {
                throw new InvalidOperationException("Cannot get sibling. Control is not included in a RowControl.");
            }
            var groupControl = rowControl.FindParent<GroupControl>();
            if (groupControl == null)
            {
                throw new InvalidOperationException("Cannot get sibling. No parent group found.");
            }
            var index = groupControl.Controls.Children.IndexOf(rowControl);
            if (index == -1)
            {
                throw new InvalidOperationException("Cannot get sibling. Did not find the rowControl in the collection.");
            }
            var rowSibling = groupControl.Controls.Children[index + n] as RowControl;
            if (rowSibling == null)
            {
                throw new InvalidOperationException("Cannot get sibling. Sibling control is of unexcpected type.");
            }
            var sibling = rowSibling.Control as ConfiguratorControl;
            if (sibling == null)
            {
                throw new InvalidOperationException("Cannot get sibling. Sibling row control has no control.");
            }
            return sibling;
        }
    }
}
