using PinJuke.Audio;
using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.Spotify;
using PinJuke.View;
using PinJuke.View.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
    public partial class MainWindow : Window
    {
        public event EventHandler? ShutdownRequestedEvent;

        public float ContentScale { get; }
        public float ContentRotation { get; }

        private bool closing = false;
        private readonly MainModel mainModel;
        public Configuration.Display DisplayConfig { get; }

        private readonly BackgroundImageControl? backgroundImageControl = null;
        private readonly VisualizerControl? visualizerControl = null;
        private readonly ThemeVideoControl? themeVideoControl = null;
        private readonly IntroImageControl introImageControl;
        private readonly MediaControl? mediaControl = null;
        private readonly CoverControl? coverControl = null;
        private readonly BrowserControl? browserControl = null;
        private readonly PlayingTrackControl? playingTrackControl = null;
        private readonly SpotifyMediaProvider? spotifyMediaProvider = null;
        private readonly SpotifyIntegrationService? spotifyIntegrationService = null;

        public MainWindow(MainModel mainModel, Configuration.Display displayConfig, AudioManager audioManager, SpotifyMediaProvider? spotifyMediaProvider = null, SpotifyIntegrationService? spotifyIntegrationService = null)
        {
            this.mainModel = mainModel;
            this.spotifyMediaProvider = spotifyMediaProvider;
            this.spotifyIntegrationService = spotifyIntegrationService;
            DisplayConfig = displayConfig;

            InitializeComponent();
            DataContext = this;

            Title = "PinJuke " + displayConfig.Role.ToString();
            Left = displayConfig.Window.Left;
            Top = displayConfig.Window.Top;
            Width = displayConfig.Window.Width;
            Height = displayConfig.Window.Height;
            ContentScale = displayConfig.Window.ContentScale;
            ContentRotation = displayConfig.Window.ContentRotation;
            Cursor = mainModel.Configuration.CursorVisible ? Cursors.Arrow : Cursors.None;

            var content = displayConfig.Content;

            if (content.PlaybackBackgroundType == Configuration.BackgroundType.Image
                || content.IdleBackgroundType == Configuration.BackgroundType.Image)
            {
                backgroundImageControl = new();
                new BackgroundImageMediator(backgroundImageControl, mainModel, displayConfig).Initialize();
                BackgroundImageContainer.Content = backgroundImageControl;
            }

            if (content.PlaybackBackgroundType == Configuration.BackgroundType.MilkdropVisualization
                || content.IdleBackgroundType == Configuration.BackgroundType.MilkdropVisualization)
            {
                visualizerControl = new();
                new VisualizerMediator(visualizerControl, mainModel, displayConfig, audioManager).Initialize();
                VisualizerContainer.Content = visualizerControl;
            }

            if (content.PlaybackBackgroundType == Configuration.BackgroundType.Video
                || content.IdleBackgroundType == Configuration.BackgroundType.Video
                || content.ThemeVideoStartFileEnabled
                || content.ThemeVideoStopFileEnabled)
            {
                themeVideoControl = new();
                new ThemeVideoMediator(themeVideoControl, mainModel, displayConfig).Initialize();
                ThemeVideoContainer.Content = themeVideoControl;
            }

            introImageControl = new();
            new IntroImageMediator(introImageControl, mainModel).Initialize();
            IntroImageContainer.Content = introImageControl;

            if (displayConfig.Role == Configuration.DisplayRole.BackGlass)
            {
                mediaControl = new();
                new MediaMediator(mediaControl, mainModel, spotifyMediaProvider).Initialize();
                MediaElementContainer.Content = mediaControl;
            }

            if (displayConfig.Content.CoverEnabled)
            {
                coverControl = new();
                new CoverMediator(coverControl, mainModel, spotifyIntegrationService).Initialize();
                CoverContainer.Content = coverControl;
            }

            if (displayConfig.Content.BrowserEnabled)
            {
                browserControl = new();
                new BrowserMediator(browserControl, mainModel).Initialize();
                BrowserContainer.Content = browserControl;
            }

            if (displayConfig.Content.StateEnabled)
            {
                playingTrackControl = new();
                new PlayingTrackMediator(playingTrackControl, mainModel).Initialize();
                PlayingTrackContainer.Content = playingTrackControl;
            }

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Title + " loaded.");
            mainModel.ShutdownEvent += MainModel_ShutdownEvent;
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            closing = true;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            Debug.WriteLine(Title + " closed.");
            mainModel.ShutdownEvent -= MainModel_ShutdownEvent;
            ShutdownRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void MainModel_ShutdownEvent(object? sender, EventArgs e)
        {
            if (closing)
            {
                return;
            }
            Debug.WriteLine(Title + " received shutdown event.");
            Close();
        }

    }
}
