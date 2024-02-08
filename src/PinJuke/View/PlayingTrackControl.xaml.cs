using PinJuke.Model;
using PinJuke.Playlist;
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
    public partial class PlayingTrackControl : BaseControl
    {
        private bool viewVisible = false;
        public bool ViewVisible
        {
            get => viewVisible;
            set => SetField(ref viewVisible, value);
        }

        private DrawingImage? stateImageSource = null;
        public DrawingImage? StateImageSource
        {
            get => stateImageSource;
            set => SetField(ref stateImageSource, value);
        }

        private string? stateText = null;
        public string? StateText
        {
            get => stateText;
            set => SetField(ref stateText, value);
        }

        public PlayingTrackControl()
        {
            InitializeComponent();
        }

    }
}
