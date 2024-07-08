using System.IO;
using System.Windows;


namespace PinJuke.Configurator.View
{
    public delegate void ButtonControlClickHandler(ButtonControl buttonControl);

    public partial class ButtonControl : ConfiguratorControl
    {
        private string text = "";
        public string Text
        {
            get => text;
            set => SetField(ref text, value);
        }

        public ButtonControlClickHandler? ClickHandler { get; set; } = null;

        public ButtonControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClickHandler?.Invoke(this);
        }
    }
}
