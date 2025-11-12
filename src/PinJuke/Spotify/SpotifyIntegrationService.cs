using PinJuke.Configuration;
using PinJuke.Model;
using PinJuke.Playlist;
using System;
using System.Collections.Generic;
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
        private MainModel? _mainModel;
        private bool _isInitialized = false;

        public bool IsEnabled => _config.Enabled && _config.IsValid;
        public bool IsConnected => _spotifyService.IsConnected;
        public ISpotifyService SpotifyService => _spotifyService;
        public SpotifyMediaProvider MediaProvider => _mediaProvider;

        public SpotifyIntegrationService(Configuration.Configuration configuration)
        {
            _config = configuration.Spotify;
            _spotifyService = new SpotifyService();
            _mediaProvider = new SpotifyMediaProvider(_config);
            
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
            System.Diagnostics.Debug.WriteLine("SpotifyIntegrationService: Setting auth result for both SpotifyService and MediaProvider");
            _spotifyService.AuthService.SetAuthResult(authResult);
            _mediaProvider.SetAuthResult(authResult);
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
            _mediaProvider?.Dispose();
            _spotifyService?.Dispose();
        }
    }
}
