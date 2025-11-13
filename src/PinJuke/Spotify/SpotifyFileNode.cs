using PinJuke.Playlist;
using System;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Specialized FileNode for Spotify content that integrates with the existing PinJuke file system
    /// </summary>
    public class SpotifyFileNode : FileNode
    {
        public SpotifyTrack? SpotifyTrack { get; }
        public SpotifyPlaylist? SpotifyPlaylist { get; }
        public string SpotifyId { get; }

        /// <summary>
        /// Creates a SpotifyFileNode for a track
        /// </summary>
        public SpotifyFileNode(SpotifyTrack track) 
            : base($"spotify:track:{track.Id}", track.DisplayName, FileType.SpotifyTrack)
        {
            SpotifyTrack = track;
            SpotifyId = track.Id;
        }

        /// <summary>
        /// Creates a SpotifyFileNode for a playlist
        /// </summary>
        public SpotifyFileNode(SpotifyPlaylist playlist) 
            : base($"spotify:playlist:{playlist.Id}", playlist.Name, FileType.SpotifyPlaylist)
        {
            SpotifyPlaylist = playlist;
            SpotifyId = playlist.Id;
        }

        /// <summary>
        /// Creates a virtual directory node for organizing Spotify content
        /// </summary>
        public SpotifyFileNode(string id, string displayName, FileType type) 
            : base($"spotify:{type.ToString().ToLower()}:{id}", displayName, type)
        {
            SpotifyId = id;
        }

        /// <summary>
        /// Gets the Spotify URI for this node
        /// </summary>
        public string SpotifyUri => SpotifyTrack?.Uri ?? SpotifyPlaylist?.Uri ?? FullName;

        /// <summary>
        /// Gets the duration for display (for tracks)
        /// </summary>
        public string? Duration => SpotifyTrack?.FormattedDuration;

        /// <summary>
        /// Gets the artist information (for tracks)
        /// </summary>
        public string? Artist => SpotifyTrack?.AllArtists;

        /// <summary>
        /// Gets the album information (for tracks)
        /// </summary>
        public string? Album => SpotifyTrack?.Album;

        /// <summary>
        /// Whether this track is playable
        /// </summary>
        public bool IsSpotifyPlayable => SpotifyTrack?.IsPlayable ?? false;

        /// <summary>
        /// Gets metadata for display purposes
        /// </summary>
        public string MetadataInfo
        {
            get
            {
                if (SpotifyTrack != null)
                {
                    return $"{SpotifyTrack.AllArtists} - {SpotifyTrack.Album} ({SpotifyTrack.FormattedDuration})";
                }
                if (SpotifyPlaylist != null)
                {
                    return $"by {SpotifyPlaylist.Owner} - {SpotifyPlaylist.TrackCount} tracks";
                }
                return string.Empty;
            }
        }
    }
}
