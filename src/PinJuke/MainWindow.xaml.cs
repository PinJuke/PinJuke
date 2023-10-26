using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
using Unosquare.FFME.Common;

namespace PinJuke
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainModel mainModel;
        private Configuration.Display displayConfig;

        private Unosquare.FFME.MediaElement? mediaElement = null;
        private MediaActionQueue? mediaActionQueue = null;

        public MainWindow(MainModel mainModel, Configuration.Display displayConfig)
        {
            this.mainModel = mainModel;
            this.displayConfig = displayConfig;

            InitializeComponent();

            Title = displayConfig.Role.ToString();
            Left = displayConfig.Window.Left;
            Top = displayConfig.Window.Top;
            Width = displayConfig.Window.Width;
            Height = displayConfig.Window.Height;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            if (displayConfig.Role == Configuration.DisplayRole.BackGlass)
            {
                mediaElement = new();
                // https://github.com/unosquare/ffmediaelement/issues/388#issuecomment-491851750
                mediaElement.RendererOptions.UseLegacyAudioOut = true;
                MediaElementContainer.Children.Add(mediaElement);
                mediaActionQueue = new(mediaElement);
            }
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
                    Browser1.FileNode = mainModel.NavigationNode;
                    break;
                case nameof(MainModel.BrowserVisible):
                    Browser1.BrowserVisible = mainModel.BrowserVisible;
                    break;
                case nameof(MainModel.PlayingFile):
                    PlayFile(mainModel.PlayingFile);
                    break;
                case nameof(MainModel.Playing):
                    SetPlayPause(mainModel.Playing);
                    break;
            }
        }

        private void PlayFile(FileNode? playingFile)
        {
            if (playingFile != null)
            {
                mediaActionQueue?.Open(playingFile.FullName);
                mediaActionQueue?.Play();
            }
            else
            {
                mediaActionQueue?.Close();
            }
        }

        private void SetPlayPause(bool playing)
        {
            if (playing)
            {
                mediaActionQueue?.Play();
            }
            else
            {
                mediaActionQueue?.Pause();
            }
        }

    }
}
