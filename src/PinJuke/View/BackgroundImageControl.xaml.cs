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
    public partial class BackgroundImageControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ImageSource? backgroundImageSource = null;
        public ImageSource? BackgroundImageSource
        {
            get => backgroundImageSource;
            set
            {
                if (value != backgroundImageSource)
                {
                    backgroundImageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private ImageSource? errorImageSource = null;
        public ImageSource? ErrorImageSource
        {
            get => errorImageSource;
            set
            {
                if (value != errorImageSource)
                {
                    errorImageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string? errorMessage = null;
        public string? ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (value != errorMessage)
                {
                    errorMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BackgroundImageControl()
        {
            InitializeComponent();
            DataContext = this;

            ErrorImageSource = SvgImageLoader.Instance.GetFromResource(@"icons\image-outline.svg");
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
