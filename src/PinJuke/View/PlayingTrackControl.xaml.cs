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
    public partial class PlayingTrackControl : BaseControl, INotifyPropertyChanged
    {

        private bool viewVisible = false;
        public bool ViewVisible
        {
            get => viewVisible;
            set => SetField(ref viewVisible, value);
        }

        private FileNode? fileNode = null;
        public FileNode? FileNode
        {
            get => fileNode;
            set
            {
                if (SetField(ref fileNode, value))
                {
                    UpdateView();
                }
            }
        }

        private bool playing = false;
        public bool Playing
        {
            get => playing;
            set
            {
                if (SetField(ref playing, value))
                {
                    UpdateView();
                }
            }
        }

        private DrawingImage? stateImageSource = null;
        public DrawingImage? StateImageSource
        {
            get => stateImageSource;
            private set => SetField(ref stateImageSource, value);
        }

        private string? trackTitle = null;
        public string? TrackTitle
        {
            get => trackTitle;
            private set => SetField(ref trackTitle, value);
        }

        public PlayingTrackControl()
        {
            InitializeComponent();
            UpdateView();
        }

        private void UpdateView()
        {
            TrackTitle = FileNode?.DisplayName;
            StateImageSource = FileNode == null
                ? SvgImageLoader.Instance.GetFromResource(@"icons\stop-outline.svg")
                : Playing
                    ? SvgImageLoader.Instance.GetFromResource(@"icons\play-outline.svg")
                    : SvgImageLoader.Instance.GetFromResource(@"icons\pause-outline.svg");
        }

    }
}
