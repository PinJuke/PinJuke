using PinJuke.Configuration;
using PinJuke.Model;
using PinJuke.Playlist;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Manages Spotify integration within the PinJuke application
    /// </summary>
    public class SpotifyIntegrationService : IDisposable
    {
        private readonly ISpotifyService _spotifyService;
        private readonly SpotifyConfig _config;
        private readonly SpotifyMediaProvider _mediaProvider;
        private readonly SpotifyPlaybackController _playbackController;
        private SpotifyStateSynchronizer? _stateSynchronizer;
        private MainModel? _mainModel;
        private bool _isInitialized = false;

        public bool IsEnabled => _config.Enabled && _config.IsValid;
        public bool IsConnected => _spotifyService.IsConnected;
        public ISpotifyService SpotifyService => _spotifyService;
        public SpotifyMediaProvider MediaProvider => _mediaProvider;
        public SpotifyPlaybackController PlaybackController => _playbackController;

        // Cache for the current active playlist to avoid reloading all playlists
        private SpotifyPlaylist? _currentPlaylistCache = null;
        private string? _currentPlaylistId = null;
        private readonly Configuration.Configuration _configuration;

        public SpotifyIntegrationService(Configuration.Configuration configuration)
        {
            _config = configuration.Spotify;
            _configuration = configuration; // Store full configuration to access Player.SpotifyPlaylistId
            _spotifyService = new SpotifyService();
            _mediaProvider = new SpotifyMediaProvider(_config, () => GetCurrentPlaylistWithTracksAsync());
            _playbackController = new SpotifyPlaybackController(_config);
            
            // Automatically load any saved authentication tokens
            try
            {
                LoadSavedAuthenticationTokens();
            }
            catch (Exception ex)
            {
                // Log but don't fail initialization if token loading fails
                System.Diagnostics.Debug.WriteLine($"SpotifyIntegrationService: Could not load saved tokens: {ex.Message}");
            }
        }

        public async Task InitializeAsync(MainModel mainModel)
        {
            if (_isInitialized)
            {
                return;
            }

            _mainModel = mainModel;
            
            // Set the Spotify integration reference in MainModel
            mainModel.SetSpotifyIntegration(this);

            if (!IsEnabled)
            {
                System.Diagnostics.Debug.WriteLine("Spotify integration is disabled or not configured.");
                return;
            }

            try
            {
                await _spotifyService.InitializeAsync(_config);
                
                // Load saved authentication tokens from global config
                LoadSavedAuthenticationTokens();
                
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("Spotify integration initialized successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize Spotify integration: {ex.Message}");
            }
        }

        /// <summary>
        /// Load saved authentication tokens from the global config file
        /// </summary>
        private void LoadSavedAuthenticationTokens()
        {
            try
            {
                var globalConfigPath = Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var iniDoc = Ini.IniReader.TryRead(globalConfigPath);
                
                if (iniDoc == null)
                {
                    System.Diagnostics.Debug.WriteLine("SpotifyIntegrationService: No global config file found");
                    return;
                }

                var spotifySection = iniDoc["Spotify"];
                var accessToken = spotifySection["AccessToken"] ?? "";
                var refreshToken = spotifySection["RefreshToken"] ?? "";
                var expiresAtStr = spotifySection["ExpiresAt"] ?? "";
                var scopesStr = spotifySection["Scopes"] ?? "";

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    System.Diagnostics.Debug.WriteLine("SpotifyIntegrationService: No saved authentication tokens found");
                    return;
                }

                // Parse expiration date
                if (!DateTime.TryParse(expiresAtStr, out var expiresAt))
                {
                    expiresAt = DateTime.Now.AddHours(-1); // Treat as expired if we can't parse
                }

                // Parse scopes
                var scopes = string.IsNullOrEmpty(scopesStr) ? Array.Empty<string>() : scopesStr.Split(',');

                // Create auth result from saved tokens
                var authResult = new SpotifyAuthResult
                {
                    IsSuccess = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    Scopes = scopes
                };

                // Set the auth result on both services
                SetAuthResult(authResult);
                System.Diagnostics.Debug.WriteLine($"SpotifyIntegrationService: Loaded saved authentication tokens (expires: {expiresAt})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SpotifyIntegrationService: Error loading saved authentication tokens: {ex.Message}");
            }
        }

        /// <summary>
        /// Set authentication result for both SpotifyService and MediaProvider
        /// This ensures both components have the same authentication
        /// </summary>
        public void SetAuthResult(SpotifyAuthResult authResult)
        {
            System.Diagnostics.Debug.WriteLine("SpotifyIntegrationService: Setting auth result for SpotifyService, MediaProvider, and PlaybackController");
            _spotifyService.AuthService.SetAuthResult(authResult);
            _mediaProvider.SetAuthResult(authResult);
            _playbackController.SetAuthResult(authResult);
            
            // Start state synchronizer if we have a valid auth result and main model
            if (authResult.IsSuccess && _mainModel != null && _stateSynchronizer == null)
            {
                _stateSynchronizer = new SpotifyStateSynchronizer(this, _mainModel);
                System.Diagnostics.Debug.WriteLine("SpotifyIntegrationService: Started Spotify state synchronizer");
            }
        }

        public async Task<bool> AuthenticateAsync()
        {
            if (!IsEnabled || !_isInitialized)
            {
                return false;
            }

            try
            {
                var result = await _spotifyService.AuthService.AuthenticateAsync();
                if (result.IsSuccess)
                {
                    // Set auth result for both SpotifyService and MediaProvider
                    SetAuthResult(result);
                    System.Diagnostics.Debug.WriteLine("SpotifyIntegrationService: Authentication successful, auth result set for both services");
                }
                return result.IsSuccess;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Spotify authentication failed: {ex.Message}");
                return false;
            }
        }

        public async Task<FileNode?> CreateSpotifyRootNodeAsync()
        {
            if (!IsConnected)
            {
                return null;
            }

            try
            {
                var spotifyRoot = new SpotifyFileNode("root", "Spotify", FileType.Directory);
                
                // Add "My Playlists" section
                var playlistsNode = new SpotifyFileNode("playlists", "My Playlists", FileType.Directory);
                var playlists = await _spotifyService.GetUserPlaylistsAsync();
                
                foreach (var playlist in playlists.Take(20)) // Limit for performance
                {
                    var playlistNode = new SpotifyFileNode(playlist);
                    playlistsNode.AppendChild(playlistNode);
                }

                spotifyRoot.AppendChild(playlistsNode);

                // Add "Liked Songs" section
                var likedSongsNode = new SpotifyFileNode("liked", "Liked Songs", FileType.Directory);
                var savedTracks = await _spotifyService.GetUserSavedTracksAsync();
                
                foreach (var track in savedTracks.Take(100)) // Limit for performance
                {
                    var trackNode = new SpotifyFileNode(track);
                    likedSongsNode.AppendChild(trackNode);
                }

                spotifyRoot.AppendChild(likedSongsNode);

                return spotifyRoot;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating Spotify root node: {ex.Message}");
                return null;
            }
        }

        public async Task<List<SpotifyFileNode>> LoadPlaylistTracksAsync(string playlistId)
        {
            if (!IsConnected)
            {
                return new List<SpotifyFileNode>();
            }

            try
            {
                var playlist = await _spotifyService.GetPlaylistAsync(playlistId);
                if (playlist == null)
                {
                    return new List<SpotifyFileNode>();
                }

                return playlist.PlayableTracks
                    .Select(track => new SpotifyFileNode(track))
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading playlist tracks: {ex.Message}");
                return new List<SpotifyFileNode>();
            }
        }

        /// <summary>
        /// Get the current active playlist with tracks loaded
        /// This is much more efficient than loading all playlists - we only load the playlist we're actually playing from
        /// </summary>
        private async Task<List<SpotifyPlaylist>> GetCurrentPlaylistWithTracksAsync()
        {
            try
            {
                if (!IsConnected)
                {
                    Trace.WriteLine("SpotifyIntegration: Not connected, cannot load current playlist");
                    return new List<SpotifyPlaylist>();
                }

                // Try to get the current playlist ID from the main model
                string? currentPlaylistId = GetCurrentPlaylistId();
                
                if (string.IsNullOrEmpty(currentPlaylistId))
                {
                    Trace.WriteLine("SpotifyIntegration: No current playlist ID available");
                    return new List<SpotifyPlaylist>();
                }

                // Check if we already have this playlist cached
                if (_currentPlaylistCache != null && _currentPlaylistId == currentPlaylistId)
                {
                    Trace.WriteLine($"SpotifyIntegration: Using cached playlist '{_currentPlaylistCache.Name}' with {_currentPlaylistCache.Tracks.Count} tracks");
                    return new List<SpotifyPlaylist> { _currentPlaylistCache };
                }

                // Load the specific playlist with tracks
                Trace.WriteLine($"SpotifyIntegration: Loading current playlist {currentPlaylistId} with tracks...");
                var fullPlaylist = await _spotifyService.GetPlaylistAsync(currentPlaylistId);
                
                if (fullPlaylist != null && fullPlaylist.Tracks.Count > 0)
                {
                    // Cache the playlist
                    _currentPlaylistCache = fullPlaylist;
                    _currentPlaylistId = currentPlaylistId;
                    
                    Trace.WriteLine($"SpotifyIntegration: Loaded current playlist '{fullPlaylist.Name}' with {fullPlaylist.Tracks.Count} tracks");
                    return new List<SpotifyPlaylist> { fullPlaylist };
                }
                else
                {
                    Trace.WriteLine($"SpotifyIntegration: Current playlist {currentPlaylistId} is empty or not found");
                    return new List<SpotifyPlaylist>();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"SpotifyIntegration: Error loading current playlist with tracks: {ex.Message}");
                return new List<SpotifyPlaylist>();
            }
        }

        /// <summary>
        /// Get the current playlist ID from configuration
        /// </summary>
        private string? GetCurrentPlaylistId()
        {
            // Get the playlist ID directly from the configuration
            // This is much more reliable than trying to guess from file nodes
            if (_configuration.Player.SourceType == Configuration.PlayerSourceType.SpotifyPlaylist &&
                !string.IsNullOrEmpty(_configuration.Player.SpotifyPlaylistId))
            {
                return _configuration.Player.SpotifyPlaylistId;
            }

            return null; // Not playing from a Spotify playlist
        }

        /// <summary>
        /// Clear the playlist cache (call this when switching playlists)
        /// </summary>
        public void ClearPlaylistCache()
        {
            _currentPlaylistCache = null;
            _currentPlaylistId = null;
            Trace.WriteLine("SpotifyIntegration: Cleared playlist cache");
        }

        public SpotifyMediaProvider GetMediaProvider()
        {
            return _mediaProvider;
        }

        public bool CanHandleFile(FileNode fileNode)
        {
            return _mediaProvider.CanHandle(fileNode);
        }

        public async Task<bool> IsTrackAvailableAsync(FileNode fileNode)
        {
            if (!CanHandleFile(fileNode))
            {
                return false;
            }

            return await _mediaProvider.IsTrackAvailableAsync(fileNode.FullName);
        }

        public void Dispose()
        {
            _stateSynchronizer?.Dispose();
            _mediaProvider?.Dispose();
            _spotifyService?.Dispose();
            _playbackController?.Dispose();
        }
    }
}
