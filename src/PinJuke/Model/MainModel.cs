using Newtonsoft.Json.Linq;
using PinJuke.Configuration;
using PinJuke.Controller;
using PinJuke.Service;
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
        Sync, // Update UI state only without triggering media events
    }

    public enum TriggerType
    {
        Playback,
        Browser,
        Automatic,
        Remote, // For remote Spotify changes
    }

    public record State(StateType Type, object? Data = null);

    public class MediaEventData
    {
    }

    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // TODO: Move these into an event bus.
        public event EventHandler? PreShutdownEvent;
        public event EventHandler? ShutdownEvent;
        public event EventHandler<InputActionEventArgs>? InputEvent;
        public event EventHandler<PresetActionEventArgs>? PresetEvent;
        public event EventHandler<PlayMediaEventArgs>? PlayMediaEvent;
        public event EventHandler<EndMediaEventArgs>? EndMediaEvent;
        public event EventHandler<MediaEventArgs<MediaEventData>>? MediaEvent;

        private PlayMediaEventArgs? playMediaEventArgs = null;
        private EndMediaEventArgs? endMediaEventArgs = null;
        private MediaEventArgs<MediaEventData>? mediaEventArgs = null;

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
        private int presetChangedByMediaAt;

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
        
        // Media controller manager for external services (Spotify, etc.)
        private readonly MediaControllerManager mediaControllerManager = new();
        
        // Track user-initiated play/pause actions to avoid double notifications from polling
        private DateTime lastUserPlayPauseAction = DateTime.MinValue;
        private const int USER_ACTION_DEBOUNCE_MS = 2000; // Skip polling notifications for 2 seconds after user action

        public MainModel(Configuration.Configuration configuration, Configuration.UserConfiguration userConfiguration)
        {
            Configuration = configuration;
            UserConfiguration = userConfiguration;
            
            // Subscribe to media controller events
            mediaControllerManager.MediaStateChanged += OnMediaControllerStateChanged;
            mediaControllerManager.TrackChanged += OnMediaControllerTrackChanged;
        }

        /// <summary>
        /// Register a media controller (e.g., Spotify, Apple Music)
        /// </summary>
        public void RegisterMediaController(IMediaController controller)
        {
            mediaControllerManager.RegisterController(controller);
        }

        /// <summary>
        /// Initialize all registered media controllers
        /// </summary>
        public async Task InitializeMediaControllersAsync()
        {
            await mediaControllerManager.InitializeAllAsync();
        }

        /// <summary>
        /// Set the playing state directly without triggering media events (for Spotify sync)
        /// </summary>
        public void SetPlayingState(bool playing)
        {
            Playing = playing;
        }

        /// <summary>
        /// Check if we should skip polling notifications due to recent user action
        /// </summary>
        public bool ShouldSkipPollingNotification()
        {
            var timeSinceLastAction = DateTime.Now - lastUserPlayPauseAction;
            return timeSinceLastAction.TotalMilliseconds < USER_ACTION_DEBOUNCE_MS;
        }

        /// <summary>
        /// Get current playlist for Spotify integration use
        /// </summary>
        public List<FileNode> GetCurrentPlaylist()
        {
            return Playlist;
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

            Debug.WriteLine("Triggering pre-shutdown event for cleanup tasks.");
            PreShutdownEvent?.Invoke(this, EventArgs.Empty);

            Debug.WriteLine("Triggering shutdown event.");
            ShuttingDown = true;
            
            // Shutdown media controllers
            Task.Run(async () =>
            {
                try
                {
                    await mediaControllerManager.ShutdownAllAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MainModel: Error shutting down media controllers: {ex.Message}");
                }
            });
            
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

        private void TriggerNextPresetByMedia()
        {
            var now = presetInfoLastHideQueuedAt = Environment.TickCount;
            if (now - presetChangedByMediaAt < 5000)
            {
                return;
            }
            presetChangedByMediaAt = now;
            PresetEvent?.Invoke(this, new(PresetAction.Next));
        }

        private UserPlaylist GetUserPlaylist()
        {
            return UserConfiguration.ProvidePlaylist(Configuration.PlaylistConfigFilePath ?? "");
        }

        public void SetScanResult(ScanResult scanResult)
        {
            Debug.WriteLine($"MainModel.SetScanResult: Root node = {scanResult.RootFileNode?.DisplayName}, Playable files = {scanResult.PlayableFileNodesByFullName.Count}");
            
            RootDirectory = scanResult.RootFileNode;

            var playlist = new List<FileNode>(scanResult.PlayableFileNodesByFullName.Values);
            Debug.WriteLine($"MainModel.SetScanResult: Created playlist with {playlist.Count} tracks");
            
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
            
            Debug.WriteLine($"MainModel.SetScanResult: Set PlayingFileIndex to {index}, Playlist count: {Playlist.Count}");

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
            Debug.WriteLine($"CheckPlayOnStartup called - PlayOnStartup: {Configuration.Player.PlayOnStartup}, SceneType: {SceneType}, PlayingFile: {PlayingFile?.DisplayName ?? "null"}");
            
            if (Configuration.Player.PlayOnStartup && SceneType == SceneType.Playback)
            {
                if (PlayingFile != null)
                {
                    // Play paused file.
                    Debug.WriteLine($"CheckPlayOnStartup: Playing paused file {PlayingFile.DisplayName}");
                    PlayFile(PlayingFile, PlayFileType.Play, TriggerType.Automatic);
                }
                else
                {
                    Debug.WriteLine("CheckPlayOnStartup: Playing next track automatically");
                    PlayNext(TriggerType.Automatic);
                }
            }
            else
            {
                Debug.WriteLine($"CheckPlayOnStartup: Not auto-playing - PlayOnStartup: {Configuration.Player.PlayOnStartup}, SceneType: {SceneType}");
            }
        }

        public void TogglePlayPause(TriggerType triggerType)
        {
            // Try to use external media controller first
            if (PlayingFile != null)
            {
                var controller = mediaControllerManager.FindControllerForFile(PlayingFile);
                if (controller != null && controller.IsConnected)
                {
                    // Track that user initiated this action
                    lastUserPlayPauseAction = DateTime.Now;
                    
                    // Show immediate UI feedback - don't wait for external service response
                    var willBePlaying = !Playing;
                    Playing = willBePlaying;
                    ShowPlaybackState(willBePlaying ? StateType.Play : StateType.Pause);
                    
                    Task.Run(async () =>
                    {
                        try
                        {
                            var success = await controller.TogglePlayPauseAsync();
                            if (!success)
                            {
                                Debug.WriteLine($"MediaController '{controller.Name}': Failed to toggle play/pause, reverting UI state");
                                // Revert UI state if external service call failed
                                _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    Playing = !willBePlaying;
                                    ShowPlaybackState(!willBePlaying ? StateType.Play : StateType.Pause);
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"MediaController '{controller.Name}': Error toggling play/pause: {ex.Message}");
                            // Revert UI state if external service call failed
                            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                Playing = !willBePlaying;
                                ShowPlaybackState(!willBePlaying ? StateType.Play : StateType.Pause);
                            });
                        }
                    });
                    return;
                }
            }

            // Use local control for non-external content
            PlayFile(PlayingFile, Playing ? PlayFileType.Pause : PlayFileType.Resume, triggerType);
        }

        public void PlayFile(FileNode? node, PlayFileType type, TriggerType triggerType, StateType? playingStateType = null)
        {
            Trace.WriteLine($"MainModel.PlayFile: Playing {node?.DisplayName ?? "null"} (Type: {type}, Trigger: {triggerType})");
            
            EnterPlayback();

            node = node?.FindThisOrNextPlayable();

            if (type == PlayFileType.Resume || type == PlayFileType.Pause)
            {
                Playing = type == PlayFileType.Resume;
            }
            else if (type == PlayFileType.Sync)
            {
                // Sync mode: Update UI state only, don't trigger media events or status overlays
                // This is used when Spotify has already changed tracks and we just need to update the local UI silently
                PlayingFile = node;
                Playing = node != null;
                GetUserPlaylist().TrackFilePath = node?.FullName ?? "";
                // Don't call ShowPlaybackState() to avoid showing status overlays during sync
                return; // Don't trigger any media events
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

            // Optional: Don't show "end media" followed by "play media" if a flipper button is used.
            if (triggerType == TriggerType.Playback)
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

            UpdateMediaPlaying(false);
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

            UpdateMediaPlaying(Playing);
        }

        private void UpdateMediaPlaying(bool mediaPlaying)
        {
            if (mediaPlaying)
            {
                MediaPlayingFile = PlayingFile;
                MediaPlaying = true;

                TriggerNextPresetByMedia();
            }
            else
            {
                MediaPlaying = false;
                MediaPlayingFile = PlayingFile;
            }
        }

        public void PlayNext(TriggerType triggerType)
        {
            // Try to use external media controller first
            if (PlayingFile != null)
            {
                var controller = mediaControllerManager.FindControllerForFile(PlayingFile);
                if (controller != null && controller.IsConnected)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var success = await controller.NextTrackAsync();
                            if (success)
                            {
                                Debug.WriteLine($"MediaController '{controller.Name}': Successfully skipped to next track");
                                // Don't update local state here - let the controller events handle it
                            }
                            else
                            {
                                Debug.WriteLine($"MediaController '{controller.Name}': Failed to skip to next track, falling back to local navigation");
                                // Fallback to local navigation
                                _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    PlayNextLocal(triggerType);
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"MediaController '{controller.Name}': Error skipping to next track: {ex.Message}");
                            // Fallback to local navigation
                            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                PlayNextLocal(triggerType);
                            });
                        }
                    });
                    return;
                }
            }

            // Use local playlist navigation for non-external content
            PlayNextLocal(triggerType);
        }

        private void PlayNextLocal(TriggerType triggerType)
        {
            // The next file can become null when the end is reached.
            var nextIndex = PlayingFile == null ? 0 : PlayingFileIndex + 1;
            var nextFile = Playlist.ElementAtOrDefault(nextIndex);
            PlayFile(nextFile, PlayFileType.Play, triggerType, StateType.Next);
        }

        public void PlayPrevious(TriggerType triggerType)
        {
            // Try to use external media controller first
            if (PlayingFile != null)
            {
                var controller = mediaControllerManager.FindControllerForFile(PlayingFile);
                if (controller != null && controller.IsConnected)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var success = await controller.PreviousTrackAsync();
                            if (success)
                            {
                                Debug.WriteLine($"MediaController '{controller.Name}': Successfully skipped to previous track");
                                // Don't update local state here - let the controller events handle it
                            }
                            else
                            {
                                Debug.WriteLine($"MediaController '{controller.Name}': Failed to skip to previous track, falling back to local navigation");
                                // Fallback to local navigation
                                _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    PlayPreviousLocal(triggerType);
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"MediaController '{controller.Name}': Error skipping to previous track: {ex.Message}");
                            // Fallback to local navigation
                            _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                PlayPreviousLocal(triggerType);
                            });
                        }
                    });
                    return;
                }
            }

            // Use local playlist navigation for non-external content
            PlayPreviousLocal(triggerType);
        }

        private void PlayPreviousLocal(TriggerType triggerType)
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

        /// <summary>
        /// Handle media state changes from external controllers
        /// </summary>
        private void OnMediaControllerStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            try
            {
                // Update local playing state silently
                SetPlayingState(e.IsPlaying);
                
                // Show notification only if requested and not from recent user action
                if (e.ShouldShowNotification && !ShouldSkipPollingNotification())
                {
                    ShowPlaybackState(e.IsPlaying ? StateType.Play : StateType.Pause);
                }
                
                Debug.WriteLine($"MediaController: State changed - Playing: {e.IsPlaying}, Track: {e.TrackName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainModel: Error handling media controller state change: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle track changes from external controllers
        /// </summary>
        private void OnMediaControllerTrackChanged(object? sender, TrackChangedEventArgs e)
        {
            try
            {
                if (e.MatchingFileNode != null)
                {
                    // Update to the new track silently
                    PlayingFile = e.MatchingFileNode;
                    Playing = e.IsPlaying;
                    GetUserPlaylist().TrackFilePath = e.MatchingFileNode.FullName ?? "";
                    
                    // Track changes should generally be silent unless specifically requested
                    if (e.ShouldShowNotification)
                    {
                        ShowPlaybackState(e.IsPlaying ? StateType.Play : StateType.Pause);
                    }
                }
                
                Debug.WriteLine($"MediaController: Track changed - {e.TrackName} by {e.ArtistName}, Playing: {e.IsPlaying}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainModel: Error handling media controller track change: {ex.Message}");
            }
        }
    }
}
