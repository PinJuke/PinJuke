﻿using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.View;
using PinJuke.View.Mediator;
using PinJuke.View.Visualizer;
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
        public event EventHandler? MediaEndedEvent;

        public float ContentScale { get; }
        public float ContentAngle { get; }

        private readonly MainModel mainModel;
        private readonly Configuration.Display displayConfig;

        private readonly BackgroundImageControl? backgroundImageControl = null;
        private readonly VisualizerControl? visualizerControl = null;
        private readonly CoverControl? coverControl = null;
        private readonly BrowserControl? browserControl = null;
        private readonly PlayingTrackControl? playingTrackControl = null;

        private readonly Unosquare.FFME.MediaElement? mediaElement = null;
        private readonly MediaActionQueue? mediaActionQueue = null;

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

            switch (displayConfig.Content.BackgroundType)
            {
                case Configuration.BackgroundType.Image:
                    backgroundImageControl = new();
                    new BackgroundImageMediator(backgroundImageControl, displayConfig).Initialize();
                    BackgroundImageContainer.Content = backgroundImageControl;
                    break;
                case Configuration.BackgroundType.MilkdropVisualization:
                    visualizerControl = new();
                    VisualizerContainer.Content = visualizerControl;
                    visualizerControl.VisualizerManager = visualizerManager;
                    break;
            }

            if (displayConfig.Role == Configuration.DisplayRole.BackGlass)
            {
                mediaElement = new();
                // https://github.com/unosquare/ffmediaelement/issues/388#issuecomment-491851750
                mediaElement.RendererOptions.UseLegacyAudioOut = true;
                mediaElement.MediaEnded += MediaElement_MediaEnded;
                MediaElementContainer.Content = mediaElement;
                mediaActionQueue = new(mediaElement);
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

            playingTrackControl = new();
            new PlayingTrackMediator(playingTrackControl, mainModel).Initialize();
            PlayingTrackContainer.Content = playingTrackControl;

            PlayFile();

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
                case nameof(MainModel.PlayingFile):
                    PlayFile();
                    break;
                case nameof(MainModel.Playing):
                    SetPlayPause();
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

        private void MediaElement_MediaEnded(object? sender, EventArgs e)
        {
            MediaEndedEvent?.Invoke(this, EventArgs.Empty);
        }

    }
}
