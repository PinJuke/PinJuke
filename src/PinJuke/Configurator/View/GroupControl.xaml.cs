using PinJuke.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinJuke.Configurator.View
{
    public partial class GroupControl : ConfiguratorControl, ContainerControl
    {
        private string labelText = "";
        public string LabelText
        {
            get => labelText;
            set => this.SetField(ref labelText, value);
        }

        Panel ContainerControl.Controls => Controls;

        public GroupControl()
        {
            InitializeComponent();
        }

        public ConfiguratorControl? FindChildByName(string name)
        {
            foreach (var child in Controls.Children)
            {
                if (child is RowControl rowControl && rowControl.Control is ConfiguratorControl configuratorControl)
                {
                    if (name == configuratorControl.Name)
                    {
                        return configuratorControl;
                    }
                }
                if (child is GroupControl groupControl)
                {
                    var groupChild = groupControl.FindChildByName(name);
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
