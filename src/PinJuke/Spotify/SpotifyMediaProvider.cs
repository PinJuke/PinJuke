using PinJuke.Playlist;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Media provider that controls Spotify playback instead of streaming directly
    /// </summary>
    public class SpotifyMediaProvider : IDisposable
    {
        private readonly SpotifyConfig config;
        private readonly SpotifyPlaybackController playbackController;
        private SpotifyAuthResult? currentAuth;
        private readonly Func<Task<List<SpotifyPlaylist>>>? getPlaylistsFunc;

        public SpotifyMediaProvider(SpotifyConfig config, Func<Task<List<SpotifyPlaylist>>>? getPlaylistsFunc = null)
        {
            this.config = config;
            this.playbackController = new SpotifyPlaybackController(config);
            this.getPlaylistsFunc = getPlaylistsFunc;
        }

        public void SetAuthResult(SpotifyAuthResult authResult)
        {
            currentAuth = authResult;
            playbackController.SetAuthResult(authResult);
        }

        private SpotifyAuthResult? GetCurrentAuth()
        {
            // Try to use cached auth first
            if (currentAuth != null && !currentAuth.IsExpired)
            {
                return currentAuth;
            }

            // Load fresh auth from config
            var freshAuth = LoadSpotifyAuthentication();
            if (freshAuth != null)
            {
                SetAuthResult(freshAuth);
            }

            return freshAuth;
        }

        /// <summary>
        /// Check if this provider can handle the given file node
        /// </summary>
        public bool CanHandle(FileNode fileNode)
        {
            bool canHandle = fileNode.Type == FileType.SpotifyTrack || 
                   (fileNode.Type == FileType.Directory && fileNode.FullName.StartsWith("spotify:"));
            
            return canHandle;
        }

        /// <summary>
        /// Instead of creating a media stream, this controls Spotify playback
        /// Returns a special marker to indicate external playback is active
        /// </summary>
        public async Task<string?> CreateMediaStreamAsync(FileNode fileNode)
        {
            try
            {
                Trace.WriteLine($"SpotifyMediaProvider: Controlling Spotify playback for {fileNode.DisplayName}");
                Trace.WriteLine($"SpotifyMediaProvider: Track URI: {fileNode.FullName}");
                Trace.WriteLine($"SpotifyMediaProvider: Config DeviceId: '{config.DeviceId}', DeviceName: '{config.DeviceName}'");
                Trace.WriteLine($"SpotifyMediaProvider: Auto-transfer enabled: {config.AutoTransferPlayback}");

                var currentAuth = GetCurrentAuth();
                if (currentAuth == null || currentAuth.IsExpired)
                {
                    Trace.WriteLine("SpotifyMediaProvider: No valid authentication available");
                    return null;
                }

                // Validate device ID
                if (string.IsNullOrEmpty(config.DeviceId))
                {
                    Trace.WriteLine("SpotifyMediaProvider: ERROR - No device selected! Please select a playback device in Global Config.");
                    return null;
                }

                // Check if this is a Spotify track from a playlist (and we have parent playlist info)
                bool isPlaylistTrack = fileNode.Type == FileType.SpotifyTrack && fileNode.Parent != null;
                
                if (isPlaylistTrack)
                {
                    Trace.WriteLine("SpotifyMediaProvider: This appears to be a track from a playlist");
                    Trace.WriteLine("SpotifyMediaProvider: For better experience, consider playing the whole playlist in Spotify app");
                    
                    // Get the playlist URI from the parent node or try to derive it
                    // For now, we'll play the individual track but log suggestions for improvement
                }

                // If auto-transfer is enabled and we have a device configured, transfer playback first
                if (config.AutoTransferPlayback && !string.IsNullOrEmpty(config.DeviceId))
                {
                    Trace.WriteLine($"SpotifyMediaProvider: Transferring playback to device {config.DeviceName} ({config.DeviceId})");
                    var transferSuccess = await playbackController.TransferPlaybackAsync(config.DeviceId, false);
                    Trace.WriteLine($"SpotifyMediaProvider: Transfer result: {transferSuccess}");
                    
                    // Wait a moment for the transfer to complete
                    await Task.Delay(500);
                }

                // Start playing the track on Spotify
                Trace.WriteLine($"SpotifyMediaProvider: Starting Spotify playback for track: {fileNode.DisplayName}");
                
                // Validate the track URI format
                if (!fileNode.FullName.StartsWith("spotify:track:"))
                {
                    Trace.WriteLine($"SpotifyMediaProvider: WARNING - Invalid track URI format: {fileNode.FullName}");
                    Trace.WriteLine("SpotifyMediaProvider: Expected format: spotify:track:TRACKID");
                    return null;
                }

                bool success = false;
                
                // For Spotify tracks, we ONLY use playlist context - no individual track playback
                if (fileNode is SpotifyFileNode spotifyTrack && spotifyTrack.SpotifyTrack != null)
                {
                    Trace.WriteLine($"SpotifyMediaProvider: Processing Spotify track: {spotifyTrack.DisplayName}");
                    Trace.WriteLine($"SpotifyMediaProvider: Track ID: {spotifyTrack.SpotifyTrack.Id}");
                    Trace.WriteLine($"SpotifyMediaProvider: getPlaylistsFunc is {(getPlaylistsFunc != null ? "available" : "null")}");
                    
                    // Try to find a playlist that contains this track
                    var (playlist, trackIndex) = await FindTrackInPlaylistsAsync(spotifyTrack.SpotifyTrack.Id);
                    
                    if (playlist != null && trackIndex >= 0)
                    {
                        // Found the track in a playlist - use playlist context for proper next/previous
                        var playlistUri = playlist.Id == "liked" ? "spotify:collection:tracks" : $"spotify:playlist:{playlist.Id}";
                        Trace.WriteLine($"SpotifyMediaProvider: Found track in playlist '{playlist.Name}' at position {trackIndex}");
                        Trace.WriteLine($"SpotifyMediaProvider: Playing playlist {playlistUri} at position {trackIndex}");
                        
                        success = await playbackController.PlayPlaylistAsync(playlistUri, config.DeviceId, trackIndex);
                        
                        if (success)
                        {
                            // Set repeat mode to context (repeat entire playlist)
                            await playbackController.SetRepeatAsync("context", config.DeviceId);
                            Trace.WriteLine("SpotifyMediaProvider: Set repeat mode to context for playlist");
                        }
                        else
                        {
                            Trace.WriteLine("SpotifyMediaProvider: PlayPlaylistAsync failed, cannot play without playlist context");
                        }
                    }
                    else
                    {
                        Trace.WriteLine("SpotifyMediaProvider: ERROR - Track not found in any accessible playlist!");
                        Trace.WriteLine("SpotifyMediaProvider: Cannot play individual tracks - playlist context required for proper next/previous");
                        Trace.WriteLine("SpotifyMediaProvider: Try playing a track from within a Spotify playlist in the app first");
                        return null; // REFUSE to play individual tracks
                    }
                }
                else
                {
                    Trace.WriteLine($"SpotifyMediaProvider: ERROR - Not a valid SpotifyFileNode: {fileNode?.GetType().Name}");
                    return null;
                }
                
                if (success)
                {
                    Trace.WriteLine($"SpotifyMediaProvider: Successfully started Spotify playback on {config.DeviceName}");
                    
                    // Set volume if configured
                    if (config.DefaultVolume > 0 && config.DefaultVolume <= 100)
                    {
                        Trace.WriteLine($"SpotifyMediaProvider: Setting volume to {config.DefaultVolume}%");
                        await playbackController.SetVolumeAsync(config.DefaultVolume, config.DeviceId);
                    }
                    
                    // Return a special marker to indicate Spotify playback is active
                    // This tells the media system that audio is being played externally
                    return "spotify:external:playback";
                }
                else
                {
                    Trace.WriteLine($"SpotifyMediaProvider: FAILED to start Spotify playback! Check:");
                    Trace.WriteLine($"  - Device '{config.DeviceName}' ({config.DeviceId}) is active in Spotify app");
                    Trace.WriteLine($"  - Spotify app is running and logged in");
                    Trace.WriteLine($"  - Track is available in a Spotify playlist");
                    Trace.WriteLine($"  - Network connection is available");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SpotifyMediaProvider error: {ex.Message}");
                Trace.WriteLine($"SpotifyMediaProvider stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Control playback state
        /// </summary>
        public async Task<bool> PauseAsync()
        {
            return await playbackController.PauseAsync(config.DeviceId);
        }

        /// <summary>
        /// Control playback state
        /// </summary>
        public async Task<bool> ResumeAsync()
        {
            return await playbackController.ResumeAsync(config.DeviceId);
        }

        /// <summary>
        /// Skip to next track
        /// </summary>
        public async Task<bool> NextTrackAsync()
        {
            return await playbackController.NextTrackAsync(config.DeviceId);
        }

        /// <summary>
        /// Skip to previous track
        /// </summary>
        public async Task<bool> PreviousTrackAsync()
        {
            return await playbackController.PreviousTrackAsync(config.DeviceId);
        }

        /// <summary>
        /// Get available devices for configuration
        /// </summary>
        public async Task<System.Collections.Generic.List<SpotifyDevice>> GetAvailableDevicesAsync()
        {
            var currentAuth = GetCurrentAuth();
            if (currentAuth == null || currentAuth.IsExpired)
            {
                Trace.WriteLine("SpotifyMediaProvider: No valid authentication for device lookup");
                return new System.Collections.Generic.List<SpotifyDevice>();
            }

            return await playbackController.GetAvailableDevicesAsync();
        }

        public Task<bool> IsTrackAvailableAsync(string trackUri)
        {
            if (string.IsNullOrEmpty(trackUri) || !trackUri.StartsWith("spotify:track:"))
            {
                return Task.FromResult(false);
            }

            var currentAuth = GetCurrentAuth();
            if (currentAuth == null || currentAuth.IsExpired)
            {
                return Task.FromResult(false);
            }

            // For remote control, we assume tracks are available if we have auth
            // In practice, availability would need to be checked via Spotify Web API
            return Task.FromResult(true);
        }

        private SpotifyAuthResult? LoadSpotifyAuthentication()
        {
            try
            {
                var globalConfigPath = PinJuke.Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var iniDoc = PinJuke.Ini.IniReader.TryRead(globalConfigPath);
                if (iniDoc == null) return null;

                var spotifySection = iniDoc["Spotify"];
                if (spotifySection == null) return null;

                var parser = new PinJuke.Configuration.Parser();
                var accessToken = parser.ParseString(spotifySection["AccessToken"]);
                var refreshToken = parser.ParseString(spotifySection["RefreshToken"]);
                var expiresAtStr = parser.ParseString(spotifySection["ExpiresAt"]);
                var scopesStr = parser.ParseString(spotifySection["Scopes"]);

                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }

                // Parse expiration date
                DateTime expiresAt = DateTime.Now.AddDays(-1); // Default to expired
                if (!string.IsNullOrEmpty(expiresAtStr) && DateTime.TryParse(expiresAtStr, out var parsedDate))
                {
                    expiresAt = parsedDate;
                }

                return new SpotifyAuthResult
                {
                    IsSuccess = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken ?? "",
                    ExpiresAt = expiresAt,
                    Scopes = !string.IsNullOrEmpty(scopesStr) ? scopesStr.Split(',') : Array.Empty<string>()
                };
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SpotifyMediaProvider: Error loading authentication: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find a track in the user's playlists and return the playlist and track index
        /// </summary>
        private async Task<(SpotifyPlaylist? playlist, int trackIndex)> FindTrackInPlaylistsAsync(string trackId)
        {
            try
            {
                var currentAuth = GetCurrentAuth();
                if (currentAuth == null || currentAuth.IsExpired)
                {
                    Trace.WriteLine("SpotifyMediaProvider: No valid auth for playlist search");
                    return (null, -1);
                }

                // Use the provided playlist function if available
                if (getPlaylistsFunc != null)
                {
                    Trace.WriteLine("SpotifyMediaProvider: Searching for track in user playlists...");
                    var playlists = await getPlaylistsFunc();
                    Trace.WriteLine($"SpotifyMediaProvider: Found {playlists.Count} playlists to search");
                    
                    foreach (var playlist in playlists)
                    {
                        Trace.WriteLine($"SpotifyMediaProvider: Checking playlist '{playlist.Name}' ({playlist.Tracks.Count} tracks)");
                        
                        // Check if this playlist contains the track
                        for (int i = 0; i < playlist.Tracks.Count; i++)
                        {
                            if (playlist.Tracks[i].Id == trackId)
                            {
                                Trace.WriteLine($"SpotifyMediaProvider: FOUND track {trackId} in playlist '{playlist.Name}' at position {i}");
                                return (playlist, i);
                            }
                        }
                    }
                }
                else
                {
                    Trace.WriteLine("SpotifyMediaProvider: No playlist function available, cannot search for track context");
                }
                
                Trace.WriteLine($"SpotifyMediaProvider: Track {trackId} not found in any of the accessible playlists");
                return (null, -1);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SpotifyMediaProvider: Error searching for track in playlists: {ex.Message}");
                return (null, -1);
            }
        }

        /// <summary>
        /// Find the index of a track within its parent playlist
        /// </summary>
        private int GetTrackIndexInPlaylist(SpotifyFileNode trackNode, SpotifyFileNode playlistNode)
        {
            try
            {
                if (trackNode.SpotifyTrack == null || playlistNode.SpotifyPlaylist == null)
                {
                    return -1;
                }

                // Find the track in the playlist's track collection
                for (int i = 0; i < playlistNode.SpotifyPlaylist.Tracks.Count; i++)
                {
                    if (playlistNode.SpotifyPlaylist.Tracks[i].Id == trackNode.SpotifyId)
                    {
                        Trace.WriteLine($"SpotifyMediaProvider: Found track '{trackNode.DisplayName}' at position {i} in playlist");
                        return i;
                    }
                }

                // If not found in the collection, try walking the FileNode children
                int childIndex = 0;
                var currentChild = playlistNode.FirstChild;
                while (currentChild != null)
                {
                    if (currentChild is SpotifyFileNode childSpotifyNode && childSpotifyNode.SpotifyId == trackNode.SpotifyId)
                    {
                        Trace.WriteLine($"SpotifyMediaProvider: Found track '{trackNode.DisplayName}' at child position {childIndex} in playlist");
                        return childIndex;
                    }
                    currentChild = currentChild.NextSibling;
                    childIndex++;
                }

                Trace.WriteLine($"SpotifyMediaProvider: Could not find track '{trackNode.DisplayName}' in playlist '{playlistNode.DisplayName}'");
                return -1;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SpotifyMediaProvider: Error finding track index: {ex.Message}");
                return -1;
            }
        }

        public void Dispose()
        {
            playbackController?.Dispose();
        }
    }
}
