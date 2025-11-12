using System;
using System.Collections.Generic;
using System.Linq;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Represents a Spotify track with essential metadata
    /// </summary>
    public class SpotifyTrack
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public int DurationMs { get; set; }
        public string Uri { get; set; } = string.Empty;
        public string PreviewUrl { get; set; } = string.Empty;
        public List<string> Artists { get; set; } = new();
        public bool IsPlayable { get; set; } = true;
        public string ExternalUrl { get; set; } = string.Empty;
        public int Popularity { get; set; }
        public List<string> AlbumImageUrls { get; set; } = new();
        public string AlbumReleaseDate { get; set; } = string.Empty;
        public string AlbumReleaseDatePrecision { get; set; } = string.Empty;

        /// <summary>
        /// Gets a display-friendly name for the track
        /// </summary>
        public string DisplayName => $"{Artist} - {Name}";

        /// <summary>
        /// Gets the duration formatted as mm:ss
        /// </summary>
        public string FormattedDuration
        {
            get
            {
                var duration = TimeSpan.FromMilliseconds(DurationMs);
                return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
            }
        }

        /// <summary>
        /// Gets all artists joined by comma
        /// </summary>
        public string AllArtists => string.Join(", ", Artists);

        /// <summary>
        /// Gets the primary album image URL
        /// </summary>
        public string AlbumImageUrl => AlbumImageUrls.FirstOrDefault() ?? string.Empty;

        /// <summary>
        /// Gets the album release year if available
        /// </summary>
        public string? AlbumYear 
        { 
            get 
            {
                if (string.IsNullOrEmpty(AlbumReleaseDate)) return null;
                
                // Try to extract year from release date
                if (DateTime.TryParse(AlbumReleaseDate, out var date))
                {
                    return date.Year.ToString();
                }
                
                // If it's just a year string
                if (AlbumReleaseDate.Length == 4 && int.TryParse(AlbumReleaseDate, out var year))
                {
                    return year.ToString();
                }
                
                // Try to extract year from formats like "2023-03-15"
                var parts = AlbumReleaseDate.Split('-');
                if (parts.Length > 0 && int.TryParse(parts[0], out var yearFromParts))
                {
                    return yearFromParts.ToString();
                }
                
                return null;
            }
        }

        public override string ToString() => DisplayName;
    }
}
