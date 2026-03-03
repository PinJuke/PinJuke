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
    public class PresetEventArgs : EventArgs
    {
        public object SelectedItem { get; }

        public PresetEventArgs(object selectedItem)
        {
            this.SelectedItem = selectedItem;
        }
    }

    public partial class PresetWindow : Window, IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<PresetEventArgs>? Selected;

        public PresetWindow()
        {
            DataContext = this;
            InitializeComponent();
            Update();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Selected?.Invoke(this, new PresetEventArgs(Presets.SelectedItem));
            Close();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            OkButton.IsEnabled = Presets.SelectedItem != null;
        }
    }
}
