using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.View;
using PinJuke.View.Visualizer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
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
using Unosquare.FFME.Common;

namespace PinJuke
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? MediaEndedEvent;

        public float ContentScale { get; }
        public float ContentAngle { get; }

        private readonly MainModel mainModel;
        private readonly Configuration.Display displayConfig;

        private Unosquare.FFME.MediaElement? mediaElement = null;
        private MediaActionQueue? mediaActionQueue = null;

        public MainWindow(MainModel mainModel, Configuration.Display displayConfig, VisualizerManager visualizerManager)
        {
            this.mainModel = mainModel;
            this.displayConfig = displayConfig;

            InitializeComponent();
            DataContext = this;

            Title = displayConfig.Role.ToString();
            Left = displayConfig.Window.Left;
            Top = displayConfig.Window.Top;
            Width = displayConfig.Window.Width;
            Height = displayConfig.Window.Height;
            ContentScale = displayConfig.Window.ContentScale;
            ContentAngle = displayConfig.Window.ContentAngle;

            VisualizerControl.VisualizerManager = visualizerManager;

            if (displayConfig.Role == Configuration.DisplayRole.BackGlass)
            {
                mediaElement = new();
                // https://github.com/unosquare/ffmediaelement/issues/388#issuecomment-491851750
                mediaElement.RendererOptions.UseLegacyAudioOut = true;
                mediaElement.MediaEnded += MediaElement_MediaEnded;
                MediaElementContainer.Children.Add(mediaElement);
                mediaActionQueue = new(mediaElement);
            }

            PlayFile();
            UpdateBrowser();
            UpdatePlayingTrack();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Title + " loaded.");
            mainModel.ShutdownEvent += MainModel_ShutdownEvent;
            mainModel.PropertyChanged += MainModel_PropertyChanged;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            Debug.WriteLine(Title + " closed.");
            mainModel.ShutdownEvent -= MainModel_ShutdownEvent;
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
        }

        private void MainModel_ShutdownEvent(object? sender, EventArgs e)
        {
            Debug.WriteLine(Title + " received shotdown event.");
            Close();
        }

        private void MainModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.NavigationNode):
                    UpdateBrowser();
                    break;
                case nameof(MainModel.BrowserVisible):
                    UpdateBrowser();
                    break;
                case nameof(MainModel.PlayingFile):
                    PlayFile();
                    UpdatePlayingTrack();
                    break;
                case nameof(MainModel.Playing):
                    SetPlayPause();
                    UpdatePlayingTrack();
                    break;
                case nameof(MainModel.PlayingTrackVisible):
                    UpdatePlayingTrack();
                    break;
            }
        }

        private void PlayFile()
        {
            if (mainModel.PlayingFile != null)
            {
                mediaActionQueue?.Open(mainModel.PlayingFile.FullName);
                SetPlayPause();
            }
            else
            {
                mediaActionQueue?.Close();
            }
        }

        private void SetPlayPause()
        {
            if (mainModel.Playing)
            {
                mediaActionQueue?.Play();
            }
            else
            {
                mediaActionQueue?.Pause();
            }
        }

        private void UpdateBrowser()
        {
            Browser.FileNode = mainModel.NavigationNode;
            Browser.ViewVisible = mainModel.BrowserVisible;
        }

        private void UpdatePlayingTrack()
        {
            PlayingTrack.FileNode = mainModel.PlayingFile;
            PlayingTrack.Playing = mainModel.Playing;
            PlayingTrack.ViewVisible = mainModel.PlayingTrackVisible;
        }

        private void MediaElement_MediaEnded(object? sender, EventArgs e)
        {
            MediaEndedEvent?.Invoke(this, EventArgs.Empty);
        }

    }
}
