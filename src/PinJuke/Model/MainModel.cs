using Newtonsoft.Json.Linq;
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
using System.Xml.Linq;

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

    public enum PlayFileType
    {
        Play,
        Resume,
        Pause,
    }

    public enum TriggerType
    {
        Manual,
        Automatic,
    }

    public record State(StateType Type, object? Data = null);

    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // TODO: Move these into an event bus.
        public event EventHandler? ShutdownEvent;
        public event EventHandler<InputActionEventArgs>? InputEvent;
        public event EventHandler<PresetActionEventArgs>? PresetEvent;
        public event EventHandler<PlayMediaEventArgs>? PlayMediaEvent;
        public event EventHandler<EndMediaEventArgs>? EndMediaEvent;

        private PlayMediaEventArgs? playMediaEventArgs = null;
        private EndMediaEventArgs? endMediaEventArgs = null;

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

        private List<FileNode> playlist = new();
        private List<FileNode> Playlist
        {
            get => playlist;
            set
            {
                var oldPlayingFile = PlayingFile;
                playlist = value;
                if (PlayingFile != oldPlayingFile)
                {
                    NotifyPropertyChanged(nameof(PlayingFile));
                }
            }
        }

        private int playingFileIndex = -1;
        private int PlayingFileIndex
        {
            get => playingFileIndex;
            set
            {
                var oldPlayingFile = PlayingFile;
                playingFileIndex = value;
                if (PlayingFile != oldPlayingFile)
                {
                    NotifyPropertyChanged(nameof(PlayingFile));
                }
            }
        }

        /// <summary>
        /// The node which is currently selected (playing or paused).
        /// </summary>
        public FileNode? PlayingFile
        {
            get => Playlist.ElementAtOrDefault(PlayingFileIndex);
            private set
            {
                if (value == PlayingFile)
                {
                    return;
                }
                PlayingFileIndex = value == null ? -1 : Playlist.IndexOf(value);
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

        private FileNode? mediaPlayingFile = null;
        /// <summary>
        /// Saves the selected file played or paused by the media control.
        /// </summary>
        public FileNode? MediaPlayingFile
        {
            get => mediaPlayingFile;
            private set
            {
                if (value == mediaPlayingFile)
                {
                    return;
                }
                mediaPlayingFile = value;
                NotifyPropertyChanged();
            }
        }

        private bool mediaPlaying = false;
        /// <summary>
        /// Saves whether the selected file is currently playing by the media control.
        /// </summary>
        public bool MediaPlaying
        {
            get => mediaPlaying;
            private set
            {
                if (value == mediaPlaying)
                {
                    return;
                }
                mediaPlaying = value;
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

        private bool presetInfoVisible = false;
        /// <summary>
        /// Saves whether the preset info overlay is currently visible.
        /// </summary>
        public bool PresetInfoVisible
        {
            get => presetInfoVisible;
            private set
            {
                if (value == presetInfoVisible)
                {
                    return;
                }
                presetInfoVisible = value;
                NotifyPropertyChanged();
            }
        }

        private int presetInfoLastHideQueuedAt;

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

        public bool ShuttingDown { get; private set; } = false;

        public MainModel(Configuration.Configuration configuration, Configuration.UserConfiguration userConfiguration)
        {
            Configuration = configuration;
            UserConfiguration = userConfiguration;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }

        public void Shutdown()
        {
            if (ShuttingDown)
            {
                Debug.WriteLine("Already shutting down.");
                return;
            }

            Debug.WriteLine("Triggering shutdown event.");
            ShuttingDown = true;
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

        public void ShowPresetInfo()
        {
            PresetInfoVisible = true;
            HidePresetInfoAfterDelay();
        }

        private async void HidePresetInfoAfterDelay()
        {
            var hideQueuedAt = presetInfoLastHideQueuedAt = Environment.TickCount;

            await Task.Delay(5000);
            if (hideQueuedAt != presetInfoLastHideQueuedAt)
            {
                return;
            }
            PresetInfoVisible = false;
        }

        public void TriggerPreviousPreset()
        {
            PresetEvent?.Invoke(this, new(PresetAction.Previous));
            ShowPresetInfo();
        }

        public void TriggerNextPreset()
        {
            PresetEvent?.Invoke(this, new(PresetAction.Next));
            ShowPresetInfo();
        }

        private UserPlaylist GetUserPlaylist()
        {
            return UserConfiguration.ProvidePlaylist(Configuration.PlaylistConfigFilePath ?? "");
        }

        public void SetScanResult(ScanResult scanResult)
        {
            RootDirectory = scanResult.RootFileNode;

            var playlist = new List<FileNode>(scanResult.PlayableFileNodesByFullName.Values);
            var index = -1;
            if (Configuration.Player.StartupTrackType == StartupTrackType.Random)
            {
                playlist.Shuffle();
            }
            if (Configuration.Player.StartupTrackType == StartupTrackType.LastPlayedTrack)
            {
                var trackFilePath = GetUserPlaylist().TrackFilePath;
                if (trackFilePath != null)
                {
                    var fileNode = scanResult.TryGetPlayableFileNodeOrDefault(trackFilePath);
                    if (fileNode != null)
                    {
                        index = playlist.IndexOf(fileNode);
                    }
                }
            }

            PlayingFileIndex = -1;
            Playlist = playlist;
            PlayingFileIndex = index;

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

        public void MediaEnded()
        {
            switch (SceneType)
            {
                case SceneType.Intro:
                    IntroEnded();
                    break;
                case SceneType.Playback:
                    PlayNext(TriggerType.Automatic);
                    break;
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
                if (PlayingFile != null)
                {
                    // Play paused file.
                    PlayFile(PlayingFile, PlayFileType.Play, TriggerType.Automatic);
                }
                else
                {
                    PlayNext(TriggerType.Automatic);
                }
            }
        }

        public void TogglePlayPause(TriggerType triggerType)
        {
            PlayFile(PlayingFile, Playing ? PlayFileType.Pause : PlayFileType.Resume, triggerType);
        }

        public void PlayFile(FileNode? node, PlayFileType type, TriggerType triggerType, StateType? playingStateType = null)
        {
            EnterPlayback();

            node = node?.FindThisOrNextPlayable();

            if (type == PlayFileType.Resume || type == PlayFileType.Pause)
            {
                Playing = type == PlayFileType.Resume;
            }
            else
            {
                // To restart a track first reset the playing track to trigger an event in any case.
                Playing = false;
                PlayingFile = null;
                PlayingFile = node;
                Playing = node != null;
            }

            ShowPlaybackState(playingStateType);
            GetUserPlaylist().TrackFilePath = node?.FullName ?? "";

            if (playMediaEventArgs != null || endMediaEventArgs != null)
            {
                return;
            }

            if (triggerType == TriggerType.Manual)
            {
                if (Playing)
                {
                    if (MediaPlaying)
                    {
                        PlayMediaFinished(type);
                    }
                    else
                    {
                        BeginPlayMedia(type);
                    }
                    return;
                }
            }

            BeginEndMedia(type);
        }

        private void BeginEndMedia(PlayFileType type)
        {
            endMediaEventArgs = new(EndMediaFinished, type);
            EndMediaEvent?.Invoke(this, endMediaEventArgs);
            endMediaEventArgs.ContinueIfNotIntercepted();
        }

        private void EndMediaFinished(PlayFileType type)
        {
            endMediaEventArgs = null;

            MediaPlaying = false;
            if (type == PlayFileType.Play)
            {
                MediaPlayingFile = null;
            }

            BeginPlayMedia(type);
        }

        private void BeginPlayMedia(PlayFileType type)
        {
            // Playing may be false if no following track is played.
            playMediaEventArgs = new (PlayMediaFinished, type);
            PlayMediaEvent?.Invoke(this, playMediaEventArgs);
            playMediaEventArgs.ContinueIfNotIntercepted();
        }

        private void PlayMediaFinished(PlayFileType type)
        {
            playMediaEventArgs = null;

            MediaPlayingFile = PlayingFile;
            MediaPlaying = Playing;
        }

        public void PlayNext(TriggerType triggerType)
        {
            // The next file can become null when the end is reached.
            var nextIndex = PlayingFile == null ? 0 : PlayingFileIndex + 1;
            var nextFile = Playlist.ElementAtOrDefault(nextIndex);
            PlayFile(nextFile, PlayFileType.Play, triggerType, StateType.Next);
        }

        public void PlayPrevious(TriggerType triggerType)
        {
            // The previous file can become null when the beginning is reached.
            var previousIndex = PlayingFile == null ? Playlist.Count - 1 : PlayingFileIndex - 1;
            var previousFile = Playlist.ElementAtOrDefault(previousIndex);
            PlayFile(previousFile, PlayFileType.Play, triggerType, StateType.Previous);
        }

        public void PlayOrFollowDirectory(TriggerType triggerType)
        {
            if (NavigationNode == null)
            {
                return;
            }

            if (NavigationNode.Playable)
            {
                PlayFile(NavigationNode, PlayFileType.Play, triggerType);
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
