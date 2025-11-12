using System;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Represents a Spotify playback device
    /// </summary>
    public class SpotifyDevice
    {
        /// <summary>
        /// Device ID used for API calls
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the device
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Device type (e.g., "Computer", "Smartphone", "Speaker")
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Whether the device is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether the device is currently restricted
        /// </summary>
        public bool IsRestricted { get; set; }

        /// <summary>
        /// Whether the device is private session
        /// </summary>
        public bool IsPrivateSession { get; set; }

        /// <summary>
        /// Current volume percentage (0-100)
        /// </summary>
        public int VolumePercent { get; set; }

        /// <summary>
        /// Whether the device supports volume control
        /// </summary>
        public bool SupportsVolume { get; set; }

        /// <summary>
        /// Display string for UI
        /// </summary>
        public string DisplayName => $"{Name} ({Type})" + (IsActive ? " [Active]" : "");

        public override string ToString() => DisplayName;
    }
}
