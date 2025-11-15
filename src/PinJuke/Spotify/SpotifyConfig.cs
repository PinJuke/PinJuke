using System;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Configuration settings for Spotify integration
    /// </summary>
    public class SpotifyConfig
    {
        /// <summary>
        /// Spotify Application Client ID - get this from Spotify Developer Dashboard
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Spotify Application Client Secret - get this from Spotify Developer Dashboard
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Redirect URI for OAuth flow - must match what's configured in Spotify app settings
        /// </summary>
        public string RedirectUri { get; set; } = "http://127.0.0.1:8888/callback";

        /// <summary>
        /// Scopes required for the application
        /// </summary>
        public string[] Scopes { get; set; } = new[]
        {
            "user-read-private",           // Read user profile
            "user-read-email",             // Read user email
            "playlist-read-private",       // Read private playlists
            "playlist-read-collaborative", // Read collaborative playlists
            "user-library-read",           // Read user's saved tracks
            "streaming",                   // Control playback
            "user-read-playback-state",    // Read current playback
            "user-read-currently-playing", // Read currently playing track
            "user-modify-playback-state"   // Control playback
        };

        /// <summary>
        /// Whether Spotify integration is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Cache duration for API responses (in minutes)
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 30;

        /// <summary>
        /// Maximum number of tracks to load per playlist
        /// </summary>
        public int MaxTracksPerPlaylist { get; set; } = 500;

        /// <summary>
        /// Whether to use Spotify's preview URLs for playback (30s clips)
        /// </summary>
        public bool UsePreviewUrls { get; set; } = false;

        /// <summary>
        /// Selected Spotify device ID for playback control
        /// </summary>
        public string? DeviceId { get; set; } = null;

        /// <summary>
        /// Selected Spotify device name (for display purposes)
        /// </summary>
        public string? DeviceName { get; set; } = null;

        /// <summary>
        /// Whether to automatically transfer playback to the selected device
        /// </summary>
        public bool AutoTransferPlayback { get; set; } = true;

        /// <summary>
        /// Default volume level (0-100) when starting playback
        /// </summary>
        public int DefaultVolume { get; set; } = 80;

        /// <summary>
        /// Stored access token for automatic authentication
        /// </summary>
        public string? AccessToken { get; set; } = null;

        /// <summary>
        /// Stored refresh token for automatic token renewal
        /// </summary>
        public string? RefreshToken { get; set; } = null;

        /// <summary>
        /// Token expiration timestamp
        /// </summary>
        public DateTime? TokenExpiresAt { get; set; } = null;

        /// <summary>
        /// Gets whether stored tokens are available
        /// </summary>
        public bool HasStoredTokens => 
            !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken);

        /// <summary>
        /// Gets whether stored tokens are still valid
        /// </summary>
        public bool HasValidStoredTokens => 
            HasStoredTokens && TokenExpiresAt.HasValue && DateTime.Now < TokenExpiresAt.Value;

        /// <summary>
        /// Validates the configuration
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(ClientId) &&
            !string.IsNullOrWhiteSpace(ClientSecret) &&
            !string.IsNullOrWhiteSpace(RedirectUri);

        /// <summary>
        /// Gets the scope string for Spotify API calls
        /// </summary>
        public string ScopeString => string.Join(" ", Scopes);
    }
}
