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

namespace PinJuke.View
{
    /// <summary>
    /// Interaction logic for BackgroundImageControl.xaml
    /// </summary>
    public partial class BackgroundImageControl : BaseControl, INotifyPropertyChanged
    {
        private ImageSource? backgroundImageSource = null;
        public ImageSource? BackgroundImageSource
        {
            get => backgroundImageSource;
            set => SetField(ref backgroundImageSource, value);
        }

        private ImageSource? errorImageSource = null;
        public ImageSource? ErrorImageSource
        {
            get => errorImageSource;
            set => SetField(ref errorImageSource, value);
        }

        private string? errorMessage = null;
        public string? ErrorMessage
        {
            get => errorMessage;
            set => SetField(ref errorMessage, value);
        }

        public BackgroundImageControl()
        {
            InitializeComponent();

            ErrorImageSource = SvgImageLoader.Instance.GetFromResource(@"icons\image-outline.svg");
        }

    }
}
