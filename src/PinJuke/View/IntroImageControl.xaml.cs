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
    public partial class IntroImageControl : BaseControl
    {
        public ImageSource ImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\pinjuke.svg");
        }

        private bool viewVisible = false;
        public bool ViewVisible
        {
            get => viewVisible;
            set => this.SetField(ref viewVisible, value);
        }

        public double ViewOpacity
        {
            get => 1.0;
        }

        public IntroImageControl()
        {
            InitializeComponent();
        }

    }
}
