using PinJuke.Model;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Synchronizes the local UI state with Spotify's remote playback state
    /// Handles cases where playback changes externally (phone, web player, etc.)
    /// </summary>
    public class SpotifyStateSynchronizer : IDisposable
    {
        private readonly SpotifyIntegrationService spotifyIntegration;
        private readonly MainModel mainModel;
        private readonly Timer pollingTimer;
        private bool disposed = false;
        
        private SpotifyCurrentlyPlaying? lastKnownState = null;
        private const int POLLING_INTERVAL_MS = 3000; // Poll every 3 seconds

        public SpotifyStateSynchronizer(SpotifyIntegrationService spotifyIntegration, MainModel mainModel)
        {
            this.spotifyIntegration = spotifyIntegration;
            this.mainModel = mainModel;
            
            // Start polling timer
            pollingTimer = new Timer(PollSpotifyState, null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(POLLING_INTERVAL_MS));
            Trace.WriteLine("SpotifyStateSynchronizer: Started polling for state changes");
        }

        private async void PollSpotifyState(object? state)
        {
            if (disposed || !spotifyIntegration.IsConnected)
            {
                return;
            }

            try
            {
                // Poll Spotify state to detect remote changes
                // We poll even if local track is not Spotify to catch remote changes
                Trace.WriteLine("SpotifyStateSynchronizer: Polling Spotify state...");
                var currentState = await spotifyIntegration.PlaybackController.GetCurrentlyPlayingAsync();
                
                if (currentState == null)
                {
                    // No playback state available
                    if (lastKnownState != null)
                    {
                        Trace.WriteLine("SpotifyStateSynchronizer: Playbook stopped remotely");
                        await UpdateUIState(null);
                    }
                    return;
                }

                // Check if state has changed significantly
                if (HasStateChanged(currentState))
                {
                    Trace.WriteLine($"SpotifyStateSynchronizer: State changed - Track: {currentState.Item?.Name}, Playing: {currentState.IsPlaying}");
                    await UpdateUIState(currentState);
                }

                lastKnownState = currentState;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SpotifyStateSynchronizer: Error polling state: {ex.Message}");
            }
        }

        private bool HasStateChanged(SpotifyCurrentlyPlaying currentState)
        {
            if (lastKnownState == null)
                return true;

            // Check if track changed
            if (currentState.Item?.Id != lastKnownState.Item?.Id)
                return true;

            // Check if play/pause state changed
            if (currentState.IsPlaying != lastKnownState.IsPlaying)
                return true;

            // Check if progress changed significantly (more than 5 seconds difference)
            var progressDiff = Math.Abs((currentState.ProgressMs ?? 0) - (lastKnownState.ProgressMs ?? 0));
            if (progressDiff > 5000)
                return true;

            return false;
        }

        private async Task UpdateUIState(SpotifyCurrentlyPlaying? spotifyState)
        {
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (spotifyState?.Item == null)
                    {
                        // Playback stopped or no track
                        if (mainModel.Playing)
                        {
                            // Use SetPlayingState to avoid triggering media events
                            mainModel.SetPlayingState(false);
                            
                            // Only show pause notification if user didn't recently initiate an action
                            if (!mainModel.ShouldSkipPollingNotification())
                            {
                                mainModel.ShowPlaybackState(StateType.Pause);
                            }
                        }
                        return;
                    }

                    // Check if we need to change tracks
                    bool trackChanged = false;
                    if (mainModel.PlayingFile is SpotifyFileNode currentSpotifyNode)
                    {
                        if (currentSpotifyNode.SpotifyId != spotifyState.Item.Id)
                        {
                            // Track changed - try to find the new track in our playlist
                            UpdateCurrentTrack(spotifyState.Item);
                            trackChanged = true;
                        }
                    }
                    else if (mainModel.PlayingFile == null || !(mainModel.PlayingFile is SpotifyFileNode))
                    {
                        // Startup case: no current track or non-Spotify track, but Spotify is playing
                        // This counts as a track change to avoid showing status overlays during initial sync
                        UpdateCurrentTrack(spotifyState.Item);
                        trackChanged = true;
                    }

                    // Update play/pause state
                    bool playStateChanged = false;
                    if (spotifyState.IsPlaying != mainModel.Playing)
                    {
                        // For remote play/pause changes, directly update the Playing state without triggering media events
                        // The play/pause already happened in Spotify - we just need to sync the local UI state
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            mainModel.SetPlayingState(spotifyState.IsPlaying);
                        });
                        playStateChanged = true;
                    }

                    // Only update playback state display for play/pause changes, not track changes
                    // Also skip notifications if user recently initiated a play/pause action
                    if (playStateChanged && !trackChanged && !mainModel.ShouldSkipPollingNotification())
                    {
                        if (spotifyState.IsPlaying)
                        {
                            mainModel.ShowPlaybackState(StateType.Play);
                        }
                        else
                        {
                            mainModel.ShowPlaybackState(StateType.Pause);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"SpotifyStateSynchronizer: Error updating UI state: {ex.Message}");
                }
            });
        }

        private void UpdateCurrentTrack(SpotifyTrack newTrack)
        {
            try
            {
                // Try to find the track in current playlist
                var playlist = mainModel.GetCurrentPlaylist();
                for (int i = 0; i < playlist.Count; i++)
                {
                    if (playlist[i] is SpotifyFileNode spotifyNode && spotifyNode.SpotifyId == newTrack.Id)
                    {
                        // Found the track in our playlist, switch to it
                        Trace.WriteLine($"SpotifyStateSynchronizer: Switching to track in playlist: {newTrack.Name}");
                        
                        // Use BeginInvoke to ensure this runs on the UI thread
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            // Use Sync mode to update UI state only without triggering media restart
                            // since Spotify is already playing the correct track
                            mainModel.PlayFile(spotifyNode, PlayFileType.Sync, TriggerType.Remote);
                        });
                        return;
                    }
                }

                // Track not found in current playlist - this happens when using queue or playing from different playlists
                // Instead of creating a temporary node that breaks things, let's just update the UI display without changing PlayingFile
                Trace.WriteLine($"SpotifyStateSynchronizer: Track not in current playlist: {newTrack.Name}");
                Trace.WriteLine($"SpotifyStateSynchronizer: This is normal when playing from queue or different playlist");
                
                // Don't create temporary nodes - they break the MainModel's playlist logic
                // The track info will be updated through other means (like cover art updates)
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SpotifyStateSynchronizer: Error updating current track: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            pollingTimer?.Dispose();
            Trace.WriteLine("SpotifyStateSynchronizer: Disposed");
        }
    }
}
