using System.IO;
using System.Windows;


namespace PinJuke.Configurator.View
{
    public partial class TextControl : ConfiguratorControl
    {
        private string value = "";
        public string Value
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

        public TextControl()
        {
            InitializeComponent();
        }
    }
}
