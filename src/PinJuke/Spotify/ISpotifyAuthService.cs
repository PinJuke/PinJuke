using System;
using System.Threading.Tasks;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Handles Spotify authentication state and tokens
    /// </summary>
    public class SpotifyAuthResult
    {
        public bool IsSuccess { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = Array.Empty<string>();

        public bool IsExpired => DateTime.Now >= ExpiresAt;
        public bool HasValidToken => IsSuccess && !string.IsNullOrEmpty(AccessToken) && !IsExpired;
    }

    /// <summary>
    /// Service for handling Spotify OAuth2 authentication
    /// </summary>
    public interface ISpotifyAuthService
    {
        /// <summary>
        /// Gets the current authentication state
        /// </summary>
        SpotifyAuthResult? CurrentAuth { get; }

        /// <summary>
        /// Whether the user is currently authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Starts the OAuth2 authentication flow
        /// </summary>
        Task<SpotifyAuthResult> AuthenticateAsync();

        /// <summary>
        /// Refreshes the access token using the refresh token
        /// </summary>
        Task<SpotifyAuthResult> RefreshTokenAsync();

        /// <summary>
        /// Logs out and clears all authentication data
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Sets the authentication result (used for restoring saved tokens)
        /// </summary>
        void SetAuthResult(SpotifyAuthResult authResult);

        /// <summary>
        /// Event fired when authentication state changes
        /// </summary>
        event EventHandler<SpotifyAuthResult> AuthenticationChanged;
    }
}
