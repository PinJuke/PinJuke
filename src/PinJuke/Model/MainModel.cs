using PinJuke.Controller;
using PinJuke.Playlist;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Model
{
    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? ShutdownEvent;
        public event EventHandler<InputActionEventArgs>? InputEvent;

        public Configuration.Configuration Configuration { get; }

        private FileNode? rootDirectory = null;
        /// <summary>
        /// The directory specified by the "MusicPath".
        /// </summary>
        public FileNode? RootDirectory
        {
            get => rootDirectory;
            set
            {
                if (value == rootDirectory)
                {
                    return;
                }
                rootDirectory = value;
                NotifyPropertyChanged();
            }
        }

        private FileNode? navigationNode = null;
        /// <summary>
        /// The node which is currenty selected in the browser.
        /// </summary>
        public FileNode? NavigationNode
        {
            get => navigationNode;
            set
            {
                if (value == navigationNode)
                {
                    return;
                }
                navigationNode = value;
                NotifyPropertyChanged();
            }
        }

        private FileNode? playingFile = null;
        /// <summary>
        /// The node which is currently selected (playing or paused).
        /// </summary>
        public FileNode? PlayingFile
        {
            get => playingFile;
            private set
            {
                if (value == playingFile)
                {
                    return;
                }
                playingFile = value;
                NotifyPropertyChanged();
            }
        }

        private bool playing = false;
        /// <summary>
        /// Saves whether the selected file is currently playing or is paused.
        /// </summary>
        public bool Playing
        {
            get => playing;
            private set
            {
                if (value == playing)
                {
                    return;
                }
                playing = value;
                NotifyPropertyChanged();
            }
        }

        private bool browserVisible = false;
        /// <summary>
        /// Saves whether the file browsing overlay is currently visible.
        /// </summary>
        public bool BrowserVisible
        {
            get => browserVisible;
            private set
            {
                if (value == browserVisible)
                {
                    return;
                }
                browserVisible = value;
                NotifyPropertyChanged();
            }
        }

        private int lastHideQueuedAt;

        public MainModel(Configuration.Configuration configuration)
        {
            Configuration = configuration;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Shutdown()
        {
            Debug.WriteLine("Triggering shutdown event.");
            ShutdownEvent?.Invoke(this, EventArgs.Empty);
        }

        public void Input(InputActionEventArgs e)
        {
            HideBrowserAfterDelay();

            InputEvent?.Invoke(this, e);
        }

        public void ShowBrowser()
        {
            BrowserVisible = true;
        }

        private async void HideBrowserAfterDelay()
        {
            var hideQueuedAt = lastHideQueuedAt = Environment.TickCount;

            await Task.Delay(5000);
            if (hideQueuedAt != lastHideQueuedAt)
            {
                return;
            }
            BrowserVisible = false;
        }

        public void NavigateNext()
        {
            var node = NavigationNode?.NextSibling;
            if (node != null)
            {
                NavigationNode = node;
            }
        }

        public void NavigatePrevious()
        {
            var node = NavigationNode?.PreviousSibling;
            if (node != null)
            {
                NavigationNode = node;
                return;
            }
            // Can we go a level up?
            node = NavigationNode?.Parent;
            if (node != null )
            {
                NavigationNode = node;
            }
        }

        public void TogglePlayPause()
        {
            Playing = !Playing;
        }

        public void PlayFile(FileNode? node)
        {
            // If a directory is passed, look for a file.
            if (node != null && !node.Playable)
            {
                node = node.GetNextPlayableInList();
            }

            // To restart a track first reset the playing track to trigger an event in any case.
            Playing = false;
            PlayingFile = null;

            PlayingFile = node;
            Playing = node != null;
        }

        public void PlayNext()
        {
            PlayFile(PlayingFile?.GetNextPlayableInList());
        }

        public void PlayPrevious()
        {
            PlayFile(PlayingFile?.GetPreviousPlayableInList());
        }

        public void DescendOrPlay()
        {
            if (NavigationNode == null)
            {
                return;
            }

            if (!NavigationNode.Playable)
            {
                if (NavigationNode.FirstChild != null)
                {
                    NavigationNode = NavigationNode.FirstChild;
                }
            }
            else
            {
                PlayFile(NavigationNode);
            }

        }
    }
}
