using System.IO;
using System.Windows;


namespace PinJuke.Configurator.View
{
    public partial class BoolControl : ConfiguratorControl
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


        public BoolControl()
        {
            InitializeComponent();
        }

    }
}
