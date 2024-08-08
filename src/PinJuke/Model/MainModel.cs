using PinJuke.Configuration;
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
    public enum StateType
    {
        Previous,
        Next,
        Play,
        Pause,
        Stop,
        Volume,
        Tilt,
    }

    public enum SceneType
    {
        Intro,
        Playback,
    }

    public record State(StateType Type, object? Data = null);

    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? ShutdownEvent;
        public event EventHandler<InputActionEventArgs>? InputEvent;

        public Configuration.Configuration Configuration { get; }
        public Configuration.UserConfiguration UserConfiguration { get; }

        private SceneType sceneType = SceneType.Intro;
        /// <summary>
        /// The current scene type to separate between modes.
        /// </summary>
        public SceneType SceneType
        {
            get => sceneType;
            private set
            {
                if (value == sceneType)
                {
                    return;
                }
                sceneType = value;
                NotifyPropertyChanged();
            }
        }

        private FileNode? rootDirectory = null;
        /// <summary>
        /// The directory specified by the "MusicPath".
        /// </summary>
        public FileNode? RootDirectory
        {
            get => rootDirectory;
            private set
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
            private set
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

        private int browserLastHideQueuedAt;

        private bool stateVisible = false;
        /// <summary>
        /// Saves whether the state overlay is currently visible.
        /// </summary>
        public bool StateVisible
        {
            get => stateVisible;
            private set
            {
                if (value == stateVisible)
                {
                    return;
                }
                stateVisible = value;
                NotifyPropertyChanged();
            }
        }

        private int stateLastHideQueuedAt;

        private bool milkdropInfoVisible = false;
        /// <summary>
        /// Saves whether the milkdrop info overlay is currently visible.
        /// </summary>
        public bool MilkdropInfoVisible
        {
            get => milkdropInfoVisible;
            private set
            {
                if (value == milkdropInfoVisible)
                {
                    return;
                }
                milkdropInfoVisible = value;
                NotifyPropertyChanged();
            }
        }

        private int milkdropInfoLastHideQueuedAt;

        private State lastState = new State(StateType.Stop);
        /// <summary>
        /// Saves the last occurred state.
        /// </summary>
        public State LastState
        {
            get => lastState;
            private set
            {
                if (value == lastState)
                {
                    return;
                }
                lastState = value;
                NotifyPropertyChanged();
            }
        }

        public MainModel(Configuration.Configuration configuration, Configuration.UserConfiguration userConfiguration)
        {
            Configuration = configuration;
            UserConfiguration = userConfiguration;
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
            HideBrowserAfterDelay();
        }

        private async void HideBrowserAfterDelay()
        {
            var hideQueuedAt = browserLastHideQueuedAt = Environment.TickCount;

            await Task.Delay(5000);
            if (hideQueuedAt != browserLastHideQueuedAt)
            {
                return;
            }
            BrowserVisible = false;
        }

        public void ShowState(State state)
        {
            LastState = state;
            StateVisible = true;
            HideStateAfterDelay();
        }

        public void ShowPlaybackState(StateType? playingStateType = null)
        {
            var stateType = PlayingFile == null
                ? StateType.Stop
                : Playing ? (playingStateType ?? StateType.Play) : StateType.Pause;
            ShowState(new State(stateType));
        }

        private async void HideStateAfterDelay()
        {
            var hideQueuedAt = stateLastHideQueuedAt = Environment.TickCount;

            await Task.Delay(3000);
            if (hideQueuedAt != stateLastHideQueuedAt)
            {
                return;
            }
            StateVisible = false;
        }

        public void ShowMilkdropInfo()
        {
            MilkdropInfoVisible = true;
            HideMilkdropInfoAfterDelay();
        }

        private async void HideMilkdropInfoAfterDelay()
        {
            var hideQueuedAt = milkdropInfoLastHideQueuedAt = Environment.TickCount;

            await Task.Delay(5000);
            if (hideQueuedAt != milkdropInfoLastHideQueuedAt)
            {
                return;
            }
            MilkdropInfoVisible = false;
        }

        private UserPlaylist GetUserPlaylist()
        {
            return UserConfiguration.ProvidePlaylist(Configuration.PlaylistConfigFilePath ?? "");
        }

        public void SetScanResult(ScanResult scanResult)
        {
            RootDirectory = scanResult.RootFileNode;

            FileNode? navigationNode = null;
            if (Configuration.Player.StartupTrackType == StartupTrackType.LastPlayedTrack)
            {
                navigationNode = scanResult.TryGetPlayableFileNodeOrDefault(GetUserPlaylist().TrackFilePath);
            }
            NavigationNode = navigationNode ?? RootDirectory?.FindChild() ?? RootDirectory;
            CheckPlayOnStartup();
        }

        public void NavigateTo(FileNode? fileNode)
        {
            NavigationNode = fileNode;
        }

        public void NavigateNext(bool repeated)
        {
            var node = NavigationNode?.NextSibling;
            if (node != null)
            {
                NavigationNode = node;
            }
            else if (!repeated)
            {
                NavigationNode = NavigationNode?.Parent?.FirstChild ?? NavigationNode;
            }
        }

        public void NavigatePrevious(bool repeated)
        {
            var node = NavigationNode?.PreviousSibling;
            if (node != null)
            {
                NavigationNode = node;
            }
            else if (!repeated)
            {
                NavigationNode = NavigationNode?.Parent?.LastChild ?? NavigationNode;
            }
        }

        public void IntroEnded()
        {
            EnterPlayback();
            CheckPlayOnStartup();
        }

        public void EnterPlayback()
        {
            SceneType = SceneType.Playback;
        }

        private void CheckPlayOnStartup()
        {
            if (Configuration.Player.PlayOnStartup && SceneType == SceneType.Playback)
            {
                PlayFile(NavigationNode);
            }
        }

        public void TogglePlayPause()
        {
            Playing = !Playing;
            ShowPlaybackState();
        }

        public void PlayFile(FileNode? node, StateType? playingStateType = null)
        {
            EnterPlayback();

            // If a directory is passed, look for a file.
            node = node?.FindThisOrNextPlayable();

            // To restart a track first reset the playing track to trigger an event in any case.
            Playing = false;
            PlayingFile = null;
            PlayingFile = node;
            Playing = node != null;
            ShowPlaybackState(playingStateType);

            GetUserPlaylist().TrackFilePath = node?.FullName ?? "";
        }

        public void PlayNext()
        {
            // The next file can become null when the end is reached.
            var nextFile = PlayingFile != null ? PlayingFile.GetNextInList() : RootDirectory;
            PlayFile(nextFile?.FindThisOrNextPlayable(), StateType.Next);
        }

        public void PlayPrevious()
        {
            // The previous file can become null when the beginning is reached.
            var previousFile = PlayingFile != null ? PlayingFile.GetPreviousInList() : RootDirectory?.GetLastInList();
            PlayFile(previousFile?.FindThisOrPreviousPlayable(), StateType.Previous);
        }

        public void PlayOrFollowDirectory()
        {
            if (NavigationNode == null)
            {
                return;
            }

            if (NavigationNode.Playable)
            {
                PlayFile(NavigationNode);
                return;
            }

            switch (NavigationNode.Type)
            {
                case FileType.Directory:
                case FileType.M3u:
                    NavigationNode = NavigationNode.FindChild() ?? NavigationNode;
                    break;
                case FileType.DirectoryUp:
                    NavigationNode = NavigationNode.FindParent() ?? NavigationNode;
                    break;
            }
        }
    }
}
