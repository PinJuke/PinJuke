using PinJuke.Playlist;
using PinJuke.Service;
using PinJuke.Spotify;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Media controller implementation for Spotify integration
    /// </summary>
    public class SpotifyMediaController : IMediaController
    {
        private readonly SpotifyIntegrationService spotifyService;
        private readonly Func<System.Collections.Generic.List<FileNode>> getCurrentPlaylist;
        
        // Track user-initiated actions to avoid double notifications
        private DateTime lastUserAction = DateTime.MinValue;
        private const int USER_ACTION_DEBOUNCE_MS = 2000;

        public bool IsConnected => spotifyService.IsConnected;
        public string Name => "Spotify";

        public event EventHandler<MediaStateChangedEventArgs>? MediaStateChanged;
        public event EventHandler<TrackChangedEventArgs>? TrackChanged;

        public SpotifyMediaController(SpotifyIntegrationService spotifyService, 
            Func<System.Collections.Generic.List<FileNode>> getCurrentPlaylist)
        {
            this.spotifyService = spotifyService ?? throw new ArgumentNullException(nameof(spotifyService));
            this.getCurrentPlaylist = getCurrentPlaylist ?? throw new ArgumentNullException(nameof(getCurrentPlaylist));
        }

        public bool CanHandle(FileNode fileNode)
        {
            return fileNode is SpotifyFileNode && IsConnected;
        }

        public async Task<bool> TogglePlayPauseAsync()
        {
            if (!IsConnected) return false;

            try
            {
                lastUserAction = DateTime.Now;
                
                // Determine the current state to decide action
                var currentState = await spotifyService.SpotifyService.GetCurrentlyPlayingAsync();
                bool isCurrentlyPlaying = currentState?.IsPlaying ?? false;

                bool success;
                if (isCurrentlyPlaying)
                {
                    success = await spotifyService.PlaybackController.PauseAsync();
                    if (success)
                    {
                        Debug.WriteLine("SpotifyMediaController: Successfully paused playback");
                        // Fire immediate state change event
                        MediaStateChanged?.Invoke(this, new MediaStateChangedEventArgs(false, currentState?.Item?.Name ?? "Unknown", true));
                    }
                }
                else
                {
                    success = await spotifyService.PlaybackController.ResumeAsync();
                    if (success)
                    {
                        Debug.WriteLine("SpotifyMediaController: Successfully resumed playback");
                        // Fire immediate state change event
                        MediaStateChanged?.Invoke(this, new MediaStateChangedEventArgs(true, currentState?.Item?.Name ?? "Unknown", true));
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyMediaController: Error toggling play/pause: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> NextTrackAsync()
        {
            if (!IsConnected) return false;

            try
            {
                var success = await spotifyService.PlaybackController.NextTrackAsync();
                if (success)
                {
                    Debug.WriteLine("SpotifyMediaController: Successfully skipped to next track");
                }
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyMediaController: Error skipping to next track: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PreviousTrackAsync()
        {
            if (!IsConnected) return false;

            try
            {
                var success = await spotifyService.PlaybackController.PreviousTrackAsync();
                if (success)
                {
                    Debug.WriteLine("SpotifyMediaController: Successfully skipped to previous track");
                }
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyMediaController: Error skipping to previous track: {ex.Message}");
                return false;
            }
        }

        public Task<bool> PlayFileAsync(FileNode fileNode)
        {
            if (!CanHandle(fileNode)) return Task.FromResult(false);

            // For Spotify, playing a specific file is handled by the existing media system
            // This method is mainly for future extensibility
            return Task.FromResult(true);
        }

        public async Task<bool> PauseAsync()
        {
            if (!IsConnected) return false;

            try
            {
                lastUserAction = DateTime.Now;
                var success = await spotifyService.PlaybackController.PauseAsync();
                if (success)
                {
                    Debug.WriteLine("SpotifyMediaController: Successfully paused playback");
                }
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyMediaController: Error pausing: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResumeAsync()
        {
            if (!IsConnected) return false;

            try
            {
                lastUserAction = DateTime.Now;
                var success = await spotifyService.PlaybackController.ResumeAsync();
                if (success)
                {
                    Debug.WriteLine("SpotifyMediaController: Successfully resumed playback");
                }
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyMediaController: Error resuming: {ex.Message}");
                return false;
            }
        }

        public async Task InitializeAsync()
        {
            // Subscribe to Spotify service events if needed
            Debug.WriteLine("SpotifyMediaController: Initialized");
            await Task.CompletedTask;
        }

        public async Task ShutdownAsync()
        {
            // Cleanup if needed
            Debug.WriteLine("SpotifyMediaController: Shutdown");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Check if we should skip notifications due to recent user action
        /// </summary>
        public bool ShouldSkipNotificationDueToUserAction()
        {
            var timeSinceLastAction = DateTime.Now - lastUserAction;
            return timeSinceLastAction.TotalMilliseconds < USER_ACTION_DEBOUNCE_MS;
        }

        /// <summary>
        /// Handle state changes from the Spotify synchronizer
        /// </summary>
        public void HandleSpotifyStateChange(bool isPlaying, string trackName, bool isTrackChange)
        {
            try
            {
                if (isTrackChange)
                {
                    // Find matching file node in current playlist
                    var playlist = getCurrentPlaylist();
                    FileNode? matchingNode = null;
                    
                    foreach (var file in playlist)
                    {
                        if (file is SpotifyFileNode spotifyNode && 
                            spotifyNode.DisplayName.Contains(trackName, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingNode = spotifyNode;
                            break;
                        }
                    }

                    TrackChanged?.Invoke(this, new TrackChangedEventArgs(
                        trackName, 
                        "Unknown Artist", 
                        isPlaying, 
                        matchingNode, 
                        false // Track changes should be silent
                    ));
                }
                else
                {
                    // Play/pause state change
                    bool shouldShowNotification = !ShouldSkipNotificationDueToUserAction();
                    MediaStateChanged?.Invoke(this, new MediaStateChangedEventArgs(
                        isPlaying, 
                        trackName, 
                        shouldShowNotification
                    ));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyMediaController: Error handling state change: {ex.Message}");
            }
        }
    }
}
