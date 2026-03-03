using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PinJuke.Configurator.View
{
    public abstract class ContainerControl : ConfiguratorControl
    {
        public abstract IList Children { get; }

        public ConfiguratorControl? FindChildByName(string name)
        {
            foreach (var eachChild in Children)
            {
                var child = eachChild;
                if (child is ConfiguratorControl configuratorControl)
                {
                    if (configuratorControl.Name == name)
                    {
                        return configuratorControl; 
                    }
                }
                if (child is ContainerControl containerControl)
                {
                    var groupChild = containerControl.FindChildByName(name);
                    if (groupChild != null)
                    {
                        return groupChild;
                    }
                }
            }
            return null;
        }

        public ConfiguratorControl GetChildByName(string name)
        {
            var child = FindChildByName(name);
            if (child == null)
            {
                throw new InvalidOperationException($"Cannot return child. Found no control for \"{name}\".");
            }
            return child;
        }
    }
}
