using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace PinJuke.Configurator.View
{
    public class Item
    {
        public string Label { get; }
        public object? Value { get; }

        public Item(string label, object? value)
        {
            Label = label;
            Value = value;
        }

        public override string ToString()
        {
            return Label;
        }
    }

    public partial class SelectControl : ConfiguratorControl
    {
        private List<Item> items = new();
        public List<Item> Items
        {
            get => items;
            set
            {
                if (!this.SetField(ref items, value))
                {
                    return;
                }
                UpdateSelected();
            }
        }

        private bool selectedByIndex = true;

        private object? selectedValue = null;
        public object? SelectedValue
        {
            get => selectedValue;
            set
            {
                selectedByIndex = false;
                if (!this.SetField(ref selectedValue, value))
                {
                    return;
                }
                UpdateSelected();
            }
        }

        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedByIndex = true;
                if (!this.SetField(ref selectedIndex, value))
                {
                    return;
                }
                UpdateSelected();
            }
        }

        private bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set => this.SetField(ref enabled, value);
        }

        public SelectControl()
        {
            InitializeComponent();
        }

        private void UpdateSelected()
        {
            if (selectedByIndex)
            {
                var selectedValue = selectedIndex >= 0 && selectedIndex < Items.Count
                    ? Items[selectedIndex].Value
                    : null;
                if (Nullable.Equals(this.selectedValue, selectedValue))
                {
                    return;
                }
                this.selectedValue = selectedValue;
                NotifyPropertyChanged(nameof(SelectedValue));
            }
            else
            {
                var selectedIndex = Items.FindIndex(item => Nullable.Equals(item.Value, selectedValue));
                if (this.selectedIndex == selectedIndex)
                {
                    return;
                }
                this.selectedIndex = selectedIndex;
                NotifyPropertyChanged(nameof(SelectedIndex));
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (selectedByIndex)
            {
                SelectedIndex = comboBox.SelectedIndex;
            }
            else
            {
                var item = (Item?)comboBox.SelectedItem;
                SelectedValue = item?.Value;
            }
        }
    }
}
