using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Main service interface for interacting with Spotify Web API
    /// </summary>
    public interface ISpotifyService : IDisposable
    {
        /// <summary>
        /// Gets the current authentication service
        /// </summary>
        ISpotifyAuthService AuthService { get; }

        /// <summary>
        /// Whether the service is currently connected to Spotify
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Initializes the service with configuration
        /// </summary>
        Task InitializeAsync(SpotifyConfig config);

        /// <summary>
        /// Gets the current user's playlists
        /// </summary>
        Task<List<SpotifyPlaylist>> GetUserPlaylistsAsync();

        /// <summary>
        /// Gets a specific playlist by ID with full track information
        /// </summary>
        Task<SpotifyPlaylist?> GetPlaylistAsync(string playlistId);

        /// <summary>
        /// Gets the user's saved tracks (liked songs)
        /// </summary>
        Task<List<SpotifyTrack>> GetUserSavedTracksAsync();

        /// <summary>
        /// Searches for tracks, albums, or playlists
        /// </summary>
        Task<List<SpotifyTrack>> SearchTracksAsync(string query, int limit = 50);

        /// <summary>
        /// Gets a playable URL for a track (preview URL or full track if available)
        /// </summary>
        Task<string?> GetTrackPlayUrlAsync(string trackId);

        /// <summary>
        /// Gets the currently playing track from Spotify
        /// </summary>
        Task<SpotifyCurrentlyPlaying?> GetCurrentlyPlayingAsync();

        /// <summary>
        /// Event fired when the connection state changes
        /// </summary>
        event EventHandler<bool> ConnectionStateChanged;
    }
}
