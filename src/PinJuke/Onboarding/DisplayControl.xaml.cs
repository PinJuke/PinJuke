using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace PinJuke.Onboarding
{
    public partial class DisplayControl : Grid, IChangingProperties
    {
        public static readonly DependencyProperty DisplayProperty =
            DependencyProperty.Register(
                "Display",
                typeof(Display),
                typeof(DisplayControl),
                new PropertyMetadata(default(Display), (d, e) => ((DisplayControl)d).NotifyPropertyChanged(nameof(Display)))
            );
        public static readonly DependencyProperty EnabledCheckedProperty =
            DependencyProperty.Register(
                "EnabledChecked",
                typeof(bool),
                typeof(DisplayControl),
                new PropertyMetadata(default(bool), (d, e) => ((DisplayControl)d).NotifyPropertyChanged(nameof(EnabledChecked)))
            );
        public static readonly DependencyProperty EnabledVisibleProperty =
            DependencyProperty.Register(
                "EnabledVisible",
                typeof(bool),
                typeof(DisplayControl),
                new PropertyMetadata(default(bool), (d, e) => ((DisplayControl)d).NotifyPropertyChanged(nameof(EnabledVisible)))
            );

        public event PropertyChangedEventHandler? PropertyChanged;

        private string displayLabelText = "";
        public string DisplayLabelText
        {
            get => displayLabelText;
            set => this.SetField(ref displayLabelText, value);
        }

        public Display Display
        {
            get => (Display)GetValue(DisplayProperty);
            set
            {
                if (Equals(value, Display))
                {
                    return;
                }
                SetValue(DisplayProperty, value);
            }
        }

        public bool EnabledChecked
        {
            get => (bool)GetValue(EnabledCheckedProperty);
            set
            {
                if (Equals(value, EnabledChecked))
                {
                    return;
                }
                SetValue(EnabledCheckedProperty, value);
            }
        }

        public bool EnabledVisible
        {
            get => (bool)GetValue(EnabledVisibleProperty);
            set
            {
                if (Equals(value, EnabledVisible))
                {
                    return;
                }
                SetValue(EnabledVisibleProperty, value);
            }
        }

        public DisplayControl()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }
    }
}
