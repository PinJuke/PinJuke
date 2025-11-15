using System;
using System.Threading.Tasks;
using PinJuke.Playlist;

namespace PinJuke.Service
{
    /// <summary>
    /// Interface for external media service controllers (Spotify, Apple Music, etc.)
    /// </summary>
    public interface IMediaController
    {
        /// <summary>
        /// Gets whether this controller is connected and available
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the name of this media controller
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines if this controller can handle the specified file
        /// </summary>
        bool CanHandle(FileNode fileNode);

        /// <summary>
        /// Toggle play/pause for the current track
        /// </summary>
        Task<bool> TogglePlayPauseAsync();

        /// <summary>
        /// Skip to next track
        /// </summary>
        Task<bool> NextTrackAsync();

        /// <summary>
        /// Skip to previous track
        /// </summary>
        Task<bool> PreviousTrackAsync();

        /// <summary>
        /// Play a specific file
        /// </summary>
        Task<bool> PlayFileAsync(FileNode fileNode);

        /// <summary>
        /// Pause playback
        /// </summary>
        Task<bool> PauseAsync();

        /// <summary>
        /// Resume playback
        /// </summary>
        Task<bool> ResumeAsync();

        /// <summary>
        /// Initialize the controller
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Shutdown the controller
        /// </summary>
        Task ShutdownAsync();

        /// <summary>
        /// Event fired when the controller detects remote state changes
        /// </summary>
        event EventHandler<MediaStateChangedEventArgs>? MediaStateChanged;

        /// <summary>
        /// Event fired when the controller detects track changes
        /// </summary>
        event EventHandler<TrackChangedEventArgs>? TrackChanged;
    }

    /// <summary>
    /// Event arguments for media state changes (play/pause)
    /// </summary>
    public class MediaStateChangedEventArgs : EventArgs
    {
        public bool IsPlaying { get; }
        public string TrackName { get; }
        public bool ShouldShowNotification { get; }

        public MediaStateChangedEventArgs(bool isPlaying, string trackName, bool shouldShowNotification = true)
        {
            IsPlaying = isPlaying;
            TrackName = trackName;
            ShouldShowNotification = shouldShowNotification;
        }
    }

    /// <summary>
    /// Event arguments for track changes
    /// </summary>
    public class TrackChangedEventArgs : EventArgs
    {
        public string TrackName { get; }
        public string ArtistName { get; }
        public bool IsPlaying { get; }
        public FileNode? MatchingFileNode { get; }
        public bool ShouldShowNotification { get; }

        public TrackChangedEventArgs(string trackName, string artistName, bool isPlaying, 
            FileNode? matchingFileNode = null, bool shouldShowNotification = false)
        {
            TrackName = trackName;
            ArtistName = artistName;
            IsPlaying = isPlaying;
            MatchingFileNode = matchingFileNode;
            ShouldShowNotification = shouldShowNotification;
        }
    }
}
