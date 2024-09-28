using PinJuke.Audio;
using PinJuke.Model;
using PinJuke.Playlist;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public float ContentScale { get; }
        public float ContentAngle { get; }

        private readonly MainModel mainModel;
        private readonly Configuration.Display displayConfig;

        private readonly BackgroundImageControl? backgroundImageControl = null;
        private readonly VisualizerControl? visualizerControl = null;
        private readonly ThemeVideoControl? themeVideoControl = null;
        private readonly MediaControl? mediaControl = null;
        private readonly CoverControl? coverControl = null;
        private readonly BrowserControl? browserControl = null;
        private readonly PlayingTrackControl? playingTrackControl = null;

        public MainWindow(MainModel mainModel, Configuration.Display displayConfig, AudioManager audioManager)
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

            switch (displayConfig.Content.BackgroundType)
            {
                case Configuration.BackgroundType.Image:
                    backgroundImageControl = new();
                    new BackgroundImageMediator(backgroundImageControl, displayConfig).Initialize();
                    BackgroundImageContainer.Content = backgroundImageControl;
                    break;
                case Configuration.BackgroundType.MilkdropVisualization:
                    visualizerControl = new();
                    new VisualizerMediator(visualizerControl, mainModel, displayConfig, audioManager).Initialize();
                    VisualizerContainer.Content = visualizerControl;
                    break;
            }

            if (!displayConfig.Content.ThemeVideoStartFile.IsNullOrEmpty()
                || !displayConfig.Content.ThemeVideoLoopFile.IsNullOrEmpty()
                || !displayConfig.Content.ThemeVideoStopFile.IsNullOrEmpty())
            {
                themeVideoControl = new();
                new ThemeVideoMediator(themeVideoControl, mainModel, displayConfig).Initialize();
                ThemeVideoContainer.Content = themeVideoControl;
            }

            if (displayConfig.Role == Configuration.DisplayRole.BackGlass)
            {
                mediaControl = new();
                new MediaMediator(mediaControl, mainModel).Initialize();
                MediaElementContainer.Content = mediaControl;
            }

            if (displayConfig.Content.CoverEnabled)
            {
                coverControl = new();
                new CoverMediator(coverControl, mainModel).Initialize();
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
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Title + " loaded.");
            mainModel.ShutdownEvent += MainModel_ShutdownEvent;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            Debug.WriteLine(Title + " closed.");
            mainModel.ShutdownEvent -= MainModel_ShutdownEvent;
        }

        private void MainModel_ShutdownEvent(object? sender, EventArgs e)
        {
            Debug.WriteLine(Title + " received shotdown event.");
            Close();
        }

    }
}
