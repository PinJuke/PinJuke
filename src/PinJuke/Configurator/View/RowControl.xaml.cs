using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinJuke.Configurator.View
{
    public partial class RowControl : ContainerControl
    {
        public override ArrayList Children { get; }

        private string labelText = "";
        public string LabelText
        {
            get => labelText;
            set => this.SetField(ref labelText, value);
        }

        public UIElement? Control
        {
            get => (UIElement?)Container.Content;
            set
            {
                Container.Content = value;
                Children.Clear();
                if (value != null)
                {
                    Children.Add(value);
                }
            }
        }

        public RowControl()
        {
            Children = new();
            InitializeComponent();
        }
    }
}
