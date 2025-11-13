using System;
using System.Collections.Generic;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Represents the current playback state from Spotify's "Get Currently Playing Track" API
    /// </summary>
    public class SpotifyCurrentlyPlaying
    {
        /// <summary>
        /// The device that is currently active
        /// </summary>
        public SpotifyDevice? Device { get; set; }

        /// <summary>
        /// Whether something is currently playing
        /// </summary>
        public bool IsPlaying { get; set; }

        /// <summary>
        /// The currently playing track or episode
        /// </summary>
        public SpotifyTrack? Item { get; set; }

        /// <summary>
        /// Progress into the currently playing track in milliseconds
        /// </summary>
        public int? ProgressMs { get; set; }

        /// <summary>
        /// Unix millisecond timestamp when data was fetched and became valid
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// The object type of the currently playing item ("track", "episode", "ad", or "unknown")
        /// </summary>
        public string CurrentlyPlayingType { get; set; } = "track";

        /// <summary>
        /// Context object (playlist, album, etc.) that contains the currently playing track
        /// </summary>
        public SpotifyPlaybackContext? Context { get; set; }

        /// <summary>
        /// Shuffle state
        /// </summary>
        public bool ShuffleState { get; set; }

        /// <summary>
        /// Repeat state ("off", "track", "context")
        /// </summary>
        public string RepeatState { get; set; } = "off";

        /// <summary>
        /// Actions allowed on the current track
        /// </summary>
        public SpotifyPlaybackActions? Actions { get; set; }
    }

    /// <summary>
    /// Represents the context (playlist, album, etc.) of the current playback
    /// </summary>
    public class SpotifyPlaybackContext
    {
        /// <summary>
        /// The object type ("artist", "playlist", "album", "show")
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The Spotify URI for the context
        /// </summary>
        public string Uri { get; set; } = string.Empty;

        /// <summary>
        /// External URLs for this context
        /// </summary>
        public Dictionary<string, string> ExternalUrls { get; set; } = new();

        /// <summary>
        /// A link to the Web API endpoint providing full details
        /// </summary>
        public string Href { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the allowed actions for the current playback
    /// </summary>
    public class SpotifyPlaybackActions
    {
        public bool InterruptingPlayback { get; set; }
        public bool Pausing { get; set; }
        public bool Resuming { get; set; }
        public bool Seeking { get; set; }
        public bool SkippingNext { get; set; }
        public bool SkippingPrev { get; set; }
        public bool TogglingRepeatContext { get; set; }
        public bool TogglingShufflee { get; set; }
        public bool TogglingRepeatTrack { get; set; }
        public bool TransferringPlayback { get; set; }
    }
}
