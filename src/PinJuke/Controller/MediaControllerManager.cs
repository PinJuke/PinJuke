using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PinJuke.Playlist;
using PinJuke.Service;

namespace PinJuke.Controller
{
    /// <summary>
    /// Manages multiple media controllers and routes commands to the appropriate one
    /// </summary>
    public class MediaControllerManager : IDisposable
    {
        private readonly List<IMediaController> controllers = new();
        
        /// <summary>
        /// Event fired when any controller reports a state change
        /// </summary>
        public event EventHandler<MediaStateChangedEventArgs>? MediaStateChanged;
        
        /// <summary>
        /// Event fired when any controller reports a track change
        /// </summary>
        public event EventHandler<TrackChangedEventArgs>? TrackChanged;

        /// <summary>
        /// Register a media controller
        /// </summary>
        public void RegisterController(IMediaController controller)
        {
            if (controllers.Contains(controller))
                return;

            controllers.Add(controller);
            
            // Subscribe to events
            controller.MediaStateChanged += OnControllerMediaStateChanged;
            controller.TrackChanged += OnControllerTrackChanged;
            
            Debug.WriteLine($"MediaControllerManager: Registered controller '{controller.Name}'");
        }

        /// <summary>
        /// Unregister a media controller
        /// </summary>
        public void UnregisterController(IMediaController controller)
        {
            if (!controllers.Contains(controller))
                return;

            // Unsubscribe from events
            controller.MediaStateChanged -= OnControllerMediaStateChanged;
            controller.TrackChanged -= OnControllerTrackChanged;
            
            controllers.Remove(controller);
            
            Debug.WriteLine($"MediaControllerManager: Unregistered controller '{controller.Name}'");
        }

        /// <summary>
        /// Initialize all registered controllers
        /// </summary>
        public async Task InitializeAllAsync()
        {
            var tasks = controllers.Select(c => InitializeController(c));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Shutdown all registered controllers
        /// </summary>
        public async Task ShutdownAllAsync()
        {
            var tasks = controllers.Select(c => ShutdownController(c));
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Find the appropriate controller for the given file
        /// </summary>
        public IMediaController? FindControllerForFile(FileNode fileNode)
        {
            return controllers.FirstOrDefault(c => c.IsConnected && c.CanHandle(fileNode));
        }

        /// <summary>
        /// Toggle play/pause using the appropriate controller
        /// </summary>
        public async Task<bool> TogglePlayPauseAsync(FileNode? currentFile)
        {
            if (currentFile == null) return false;
            
            var controller = FindControllerForFile(currentFile);
            if (controller == null) return false;

            return await controller.TogglePlayPauseAsync();
        }

        /// <summary>
        /// Skip to next track using the appropriate controller
        /// </summary>
        public async Task<bool> NextTrackAsync(FileNode? currentFile)
        {
            if (currentFile == null) return false;
            
            var controller = FindControllerForFile(currentFile);
            if (controller == null) return false;

            return await controller.NextTrackAsync();
        }

        /// <summary>
        /// Skip to previous track using the appropriate controller
        /// </summary>
        public async Task<bool> PreviousTrackAsync(FileNode? currentFile)
        {
            if (currentFile == null) return false;
            
            var controller = FindControllerForFile(currentFile);
            if (controller == null) return false;

            return await controller.PreviousTrackAsync();
        }

        /// <summary>
        /// Play a specific file using the appropriate controller
        /// </summary>
        public async Task<bool> PlayFileAsync(FileNode fileNode)
        {
            var controller = FindControllerForFile(fileNode);
            if (controller == null) return false;

            return await controller.PlayFileAsync(fileNode);
        }

        private async Task InitializeController(IMediaController controller)
        {
            try
            {
                await controller.InitializeAsync();
                Debug.WriteLine($"MediaControllerManager: Initialized controller '{controller.Name}'");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MediaControllerManager: Failed to initialize controller '{controller.Name}': {ex.Message}");
            }
        }

        private async Task ShutdownController(IMediaController controller)
        {
            try
            {
                await controller.ShutdownAsync();
                Debug.WriteLine($"MediaControllerManager: Shutdown controller '{controller.Name}'");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MediaControllerManager: Failed to shutdown controller '{controller.Name}': {ex.Message}");
            }
        }

        private void OnControllerMediaStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            MediaStateChanged?.Invoke(sender, e);
        }

        private void OnControllerTrackChanged(object? sender, TrackChangedEventArgs e)
        {
            TrackChanged?.Invoke(sender, e);
        }

        public void Dispose()
        {
            // Clean up any resources if needed
            foreach (var controller in controllers)
            {
                if (controller is IDisposable disposableController)
                {
                    disposableController.Dispose();
                }
            }
            controllers.Clear();
        }
    }
}
