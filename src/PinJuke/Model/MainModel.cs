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
            InputEvent?.Invoke(this, e);
        }

        public void ShowBrowser()
        {
            BrowserVisible = true;
        }

        public void PlayNext()
        {
            var node = PlayingFile?.GetNextPlayableInList();
            if (node != null)
            {
                PlayingFile = node;
            }
        }

        public void PlayPrevious()
        {
            var node = PlayingFile?.GetPreviousPlayableInList();
            if (node != null)
            {
                PlayingFile = node;
            }
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
            Playing = false;
            PlayingFile = node;
            Playing = node != null;
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
                if (NavigationNode == PlayingFile)
                {
                    TogglePlayPause();
                }
                else
                {
                    PlayFile(NavigationNode);
                }
            }

        }
    }
}
