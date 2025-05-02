using System.IO;
using System.Windows;


namespace PinJuke.Configurator.View
{
    public partial class BoolControl : ConfiguratorControl
    {
        private bool value = false;
        public bool Value
        {
            get => value;
            set => this.SetField(ref this.value, value);
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


        public BoolControl()
        {
            InitializeComponent();
        }

    }
}
