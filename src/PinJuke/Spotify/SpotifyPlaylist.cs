using System;
using System.Collections.Generic;
using System.Linq;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Represents a Spotify playlist with its metadata and tracks
    /// </summary>
    public class SpotifyPlaylist
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public int TrackCount { get; set; }
        public bool IsPublic { get; set; }
        public bool IsCollaborative { get; set; }
        public string Uri { get; set; } = string.Empty;
        public string ExternalUrl { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();
        public List<SpotifyTrack> Tracks { get; set; } = new();

        /// <summary>
        /// Gets the primary image URL for the playlist
        /// </summary>
        public string PrimaryImageUrl => ImageUrls.FirstOrDefault() ?? string.Empty;

        /// <summary>
        /// Gets a display-friendly description of the playlist
        /// </summary>
        public string DisplayInfo => $"{Name} by {Owner} ({TrackCount} tracks)";

        /// <summary>
        /// Gets only playable tracks from the playlist
        /// </summary>
        public List<SpotifyTrack> PlayableTracks => Tracks.Where(t => t.IsPlayable).ToList();

        public override string ToString() => Name;
    }
}
