using PinJuke.Playlist;
using System;
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

        public SpotifyMediaProvider(SpotifyConfig config)
        {
            this.config = config;
            this.playbackController = new SpotifyPlaybackController(config);
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
                Debug.WriteLine($"SpotifyMediaProvider: Controlling Spotify playback for {fileNode.DisplayName}");
                Debug.WriteLine($"SpotifyMediaProvider: Track URI: {fileNode.FullName}");
                Debug.WriteLine($"SpotifyMediaProvider: Config DeviceId: '{config.DeviceId}', DeviceName: '{config.DeviceName}'");
                Debug.WriteLine($"SpotifyMediaProvider: Auto-transfer enabled: {config.AutoTransferPlayback}");

                var currentAuth = GetCurrentAuth();
                if (currentAuth == null || currentAuth.IsExpired)
                {
                    Debug.WriteLine("SpotifyMediaProvider: No valid authentication available");
                    return null;
                }

                // Validate device ID
                if (string.IsNullOrEmpty(config.DeviceId))
                {
                    Debug.WriteLine("SpotifyMediaProvider: ERROR - No device selected! Please select a playback device in Global Config.");
                    return null;
                }

                // Check if this is a Spotify track from a playlist (and we have parent playlist info)
                bool isPlaylistTrack = fileNode.Type == FileType.SpotifyTrack && fileNode.Parent != null;
                
                if (isPlaylistTrack)
                {
                    Debug.WriteLine("SpotifyMediaProvider: This appears to be a track from a playlist");
                    Debug.WriteLine("SpotifyMediaProvider: For better experience, consider playing the whole playlist in Spotify app");
                    
                    // Get the playlist URI from the parent node or try to derive it
                    // For now, we'll play the individual track but log suggestions for improvement
                }

                // If auto-transfer is enabled and we have a device configured, transfer playback first
                if (config.AutoTransferPlayback && !string.IsNullOrEmpty(config.DeviceId))
                {
                    Debug.WriteLine($"SpotifyMediaProvider: Transferring playback to device {config.DeviceName} ({config.DeviceId})");
                    var transferSuccess = await playbackController.TransferPlaybackAsync(config.DeviceId, false);
                    Debug.WriteLine($"SpotifyMediaProvider: Transfer result: {transferSuccess}");
                    
                    // Wait a moment for the transfer to complete
                    await Task.Delay(500);
                }

                // Start playing the track on Spotify
                Debug.WriteLine($"SpotifyMediaProvider: Starting track playback on device {config.DeviceId}...");
                
                // Validate the track URI format
                if (!fileNode.FullName.StartsWith("spotify:track:"))
                {
                    Debug.WriteLine($"SpotifyMediaProvider: WARNING - Invalid track URI format: {fileNode.FullName}");
                    Debug.WriteLine("SpotifyMediaProvider: Expected format: spotify:track:TRACKID");
                    return null;
                }
                
                var success = await playbackController.PlayTrackAsync(fileNode.FullName, config.DeviceId);
                
                if (success)
                {
                    Debug.WriteLine($"SpotifyMediaProvider: Successfully started Spotify playback on {config.DeviceName}");
                    
                    // Set volume if configured
                    if (config.DefaultVolume > 0 && config.DefaultVolume <= 100)
                    {
                        Debug.WriteLine($"SpotifyMediaProvider: Setting volume to {config.DefaultVolume}%");
                        await playbackController.SetVolumeAsync(config.DefaultVolume, config.DeviceId);
                    }
                    
                    // Return a special marker to indicate Spotify playback is active
                    // This tells the media system that audio is being played externally
                    return "spotify:external:playback";
                }
                else
                {
                    Debug.WriteLine($"SpotifyMediaProvider: FAILED to start Spotify playback! Check:");
                    Debug.WriteLine($"  - Device '{config.DeviceName}' ({config.DeviceId}) is active in Spotify app");
                    Debug.WriteLine($"  - Spotify app is running and logged in");
                    Debug.WriteLine($"  - Track URI '{fileNode.FullName}' is valid");
                    Debug.WriteLine($"  - Network connection is available");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyMediaProvider error: {ex.Message}");
                Debug.WriteLine($"SpotifyMediaProvider stack trace: {ex.StackTrace}");
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
                Debug.WriteLine("SpotifyMediaProvider: No valid authentication for device lookup");
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
                Debug.WriteLine($"SpotifyMediaProvider: Error loading authentication: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            playbackController?.Dispose();
        }
    }
}
