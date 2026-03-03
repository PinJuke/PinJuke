using PinJuke.Controller;
using PinJuke.Playlist;
using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PinJuke.Configurator
{
    public partial class KeyboardCaptureWindow : Window, IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<KeyEventArgs>? KeyPressed;

        public KeyboardCaptureWindow()
        {
            DataContext = this;
            InitializeComponent();

            KeyDown += Window_KeyDown;
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Only handle simple keys
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                || e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                return;
            }

            e.Handled = true;
            KeyPressed?.Invoke(this, e);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
