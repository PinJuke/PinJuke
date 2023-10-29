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
    public partial class PlayingTrack : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool viewVisible = false;
        public bool ViewVisible
        {
            get => viewVisible;
            set
            {
                if (value != viewVisible)
                {
                    viewVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private FileNode? fileNode = null;
        public FileNode? FileNode
        {
            get => fileNode;
            set
            {
                if (value != fileNode)
                {
                    fileNode = value;
                    UpdateView();
                    NotifyPropertyChanged();
                }
            }
        }

        private bool playing = false;
        public bool Playing
        {
            get => playing;
            set
            {
                if (value != playing)
                {
                    playing = value;
                    UpdateView();
                    NotifyPropertyChanged();
                }
            }
        }

        private DrawingImage? stateImageSource = null;
        public DrawingImage? StateImageSource
        {
            get => stateImageSource;
            private set
            {
                if (value != stateImageSource)
                {
                    stateImageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string? trackTitle = null;
        public string? TrackTitle
        {
            get => trackTitle;
            private set
            {
                if (value != trackTitle)
                {
                    trackTitle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PlayingTrack()
        {
            InitializeComponent();
            DataContext = this;
            UpdateView();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
