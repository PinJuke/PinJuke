using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace PinJuke.Configurator.View
{
    public partial class BoolControl : ConfiguratorControl, ContainerControl
    {
        private bool value = true;
        public bool Value
        {
            get => value;
            set
            {
                if (this.SetField(ref this.value, value))
                {
                    OnChanged();
                }
            }
        }

        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set => this.SetField(ref enabled, value);
        }


        public string LabelText
        {
            get => "";
        }

        Panel ContainerControl.Controls => Controls;

        public BoolControl()
        {
            InitializeComponent();
        }

    }
}
