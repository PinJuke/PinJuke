using PinJuke.Onboarding;
using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PinJuke.Configurator
{
    public partial class IconButtonControl : Button, IChangingProperties
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                "ImageSource",
                typeof(ImageSource),
                typeof(IconButtonControl),
                new PropertyMetadata(default(ImageSource), (d, e) => ((IconButtonControl)d).NotifyPropertyChanged(nameof(ImageSource)))
            );
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register(
                "LabelText",
                typeof(string),
                typeof(IconButtonControl),
                new PropertyMetadata(default(string), (d, e) => ((IconButtonControl)d).NotifyPropertyChanged(nameof(LabelText)))
            );

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageSource? ImageSource
        {
            get => (ImageSource?)GetValue(ImageSourceProperty);
            set
            {
                if (Equals(value, ImageSource))
                {
                    return;
                }
                SetValue(ImageSourceProperty, value);
            }
        }

        public string? LabelText
        {
            get => (string?)GetValue(LabelTextProperty);
            set
            {
                if (Equals(value, LabelText))
                {
                    return;
                }
                SetValue(LabelTextProperty, value);
            }
        }

        public IconButtonControl()
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
