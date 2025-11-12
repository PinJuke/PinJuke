#if !DISABLE_DIRECTOUTPUT
using DirectOutput.Cab.Out.DMX;
#endif
using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.Spotify;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PinJuke.View.Mediator
{
    public class CoverMediator : Mediator
    {
        private readonly CoverControl coverControl;
        private readonly MainModel mainModel;
        private readonly SpotifyIntegrationService? spotifyIntegration;
        private readonly DispatcherTimer spotifyUpdateTimer;
        private string? currentExpectedTrackUri; // Track the currently expected Spotify track

        public CoverMediator(CoverControl coverControl, MainModel mainModel, SpotifyIntegrationService? spotifyIntegration = null) : base(coverControl)
        {
            this.coverControl = coverControl;
            this.mainModel = mainModel;
            this.spotifyIntegration = spotifyIntegration;
            
            // Setup timer for periodic Spotify updates
            spotifyUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) // Update every 5 seconds
            };
            spotifyUpdateTimer.Tick += SpotifyUpdateTimer_Tick;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            
            UpdateView();
            UpdateVisibility();
            mainModel.PropertyChanged += MainModel_PropertyChanged;
            
            // Start timer for Spotify updates if integration is available
            if (spotifyIntegration?.IsEnabled == true)
            {
                spotifyUpdateTimer.Start();
            }
        }

        protected override void OnUnloaded()
        {
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
            spotifyUpdateTimer.Stop();
            base.OnUnloaded();
        }

        private async void SpotifyUpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Update for any Spotify-related playback
            if (mainModel.MediaPlayingFile is SpotifyFileNode || 
                (mainModel.MediaPlayingFile != null && spotifyIntegration?.IsEnabled == true))
            {
                await UpdateSpotifyTrackInfo();
            }
        }

        private void MainModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.SceneType):
                    UpdateVisibility();
                    break;
                case nameof(MainModel.PlayingFile):
                case nameof(MainModel.MediaPlayingFile):
                    UpdateView();
                    break;
            }
        }

        private void UpdateVisibility()
        {
            coverControl.Visibility = mainModel.SceneType == SceneType.Playback ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        private async void UpdateView()
        {
            try
            {
                // Add detailed type checking
                if (mainModel.MediaPlayingFile != null)
                {
                    if (mainModel.MediaPlayingFile is SpotifyFileNode)
                    {
                        // Will be handled in the SpotifyFileNode section below
                    }
                    else
                    {
                        // Check if this is actually a Spotify track in disguise
                        if (mainModel.MediaPlayingFile.FullName?.Contains("spotify:") == true)
                        {
                            // Update the expected track URI
                            currentExpectedTrackUri = mainModel.MediaPlayingFile.FullName;
                            
                            // Clear previous album art immediately
                            coverControl.CoverImageSource = null;
                            
                            // Treat this as a Spotify track even though it's not the right type
                            await HandleSpotifyTrackFromFileNode(mainModel.MediaPlayingFile);
                            
                            // Delay before updating to allow Spotify API to catch up
                            _ = Task.Run(async () => 
                            {
                                await Task.Delay(2000); // Wait 2 seconds for API to update
                                await UpdateSpotifyTrackInfo();
                            });
                            return;
                        }
                    }
                }
                
                if (mainModel.MediaPlayingFile is SpotifyFileNode spotifyNode)
                {
                    // Update the expected track URI
                    currentExpectedTrackUri = spotifyNode.SpotifyTrack?.Uri;
                    
                    // Clear previous album art immediately
                    coverControl.CoverImageSource = null;
                    
                    // Delay before updating to allow Spotify API to catch up
                    _ = Task.Run(async () => 
                    {
                        await Task.Delay(2000); // Wait 2 seconds for API to update
                        await UpdateSpotifyTrackInfo();
                    });
                    
                    // First try to get real-time metadata from Spotify API
                    var currentlyPlaying = await GetCurrentlyPlayingFromSpotifyAsync();
                    if (currentlyPlaying?.Item != null)
                    {
                        // Update track info from real-time API
                        coverControl.TitleText = currentlyPlaying.Item.Name;
                        coverControl.ArtistText = currentlyPlaying.Item.Artist;
                        coverControl.AlbumText = currentlyPlaying.Item.Album;
                        
                        // Show year if available
                        if (!string.IsNullOrEmpty(currentlyPlaying.Item.AlbumYear))
                        {
                            coverControl.YearText = currentlyPlaying.Item.AlbumYear;
                        }
                        else
                        {
                            coverControl.YearText = "";
                        }
                        
                        // Load album art from real-time API
                        if (!string.IsNullOrEmpty(currentlyPlaying.Item.AlbumImageUrl))
                        {
                            await LoadSpotifyAlbumArtFromTrackAsync(currentlyPlaying.Item);
                        }
                    }
                    else
                    {
                        // Fall back to cached track information
                        if (spotifyNode.SpotifyTrack != null)
                        {
                            coverControl.TitleText = spotifyNode.SpotifyTrack.Name;
                            coverControl.ArtistText = spotifyNode.SpotifyTrack.Artist;
                            coverControl.AlbumText = spotifyNode.SpotifyTrack.Album;
                            
                            if (!string.IsNullOrEmpty(spotifyNode.SpotifyTrack.AlbumYear))
                            {
                                coverControl.YearText = spotifyNode.SpotifyTrack.AlbumYear;
                            }
                            else
                            {
                                coverControl.YearText = "";
                            }
                            
                            // Load cached album art
                            if (!string.IsNullOrEmpty(spotifyNode.SpotifyTrack.AlbumImageUrl))
                            {
                                await LoadSpotifyAlbumArtFromTrackAsync(spotifyNode.SpotifyTrack);
                            }
                        }
                    }
                }
                else if (mainModel.MediaPlayingFile != null)
                {
                    // Handle regular files
                    var path = mainModel.MediaPlayingFile.FullName;
                    if (File.Exists(path))
                    {
                        using var file = TagLib.File.Create(path);
                        coverControl.TitleText = file.Tag.Title ?? Path.GetFileNameWithoutExtension(path);
                        coverControl.ArtistText = file.Tag.FirstPerformer ?? "";
                        coverControl.AlbumText = file.Tag.Album ?? "";
                        
                        if (file.Tag.Year > 0)
                        {
                            coverControl.YearText = file.Tag.Year.ToString();
                        }
                        else
                        {
                            coverControl.YearText = "";
                        }

                        // Load album art from file
                        if (file.Tag.Pictures?.Length > 0)
                        {
                            var picture = file.Tag.Pictures[0];
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = new MemoryStream(picture.Data.Data);
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.Freeze();

                            coverControl.CoverImageSource = image;
                        }
                        else
                        {
                            coverControl.CoverImageSource = null;
                        }
                    }
                }
                else
                {
                    // No file playing, clear the display
                    coverControl.TitleText = "";
                    coverControl.ArtistText = "";
                    coverControl.AlbumText = "";
                    coverControl.YearText = "";
                    coverControl.CoverImageSource = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CoverMediator.UpdateView: Error updating cover view: {ex.Message}");
            }
        }

        private async Task<SpotifyCurrentlyPlaying?> GetCurrentlyPlayingFromSpotifyAsync()
        {
            try
            {
                if (spotifyIntegration?.IsEnabled == true && spotifyIntegration.SpotifyService != null)
                {
                    try
                    {
                        return await spotifyIntegration.SpotifyService.GetCurrentlyPlayingAsync();
                    }
                    catch (InvalidOperationException authEx) when (authEx.Message.Contains("Not authenticated"))
                    {
                        // Authentication issue - this can happen occasionally
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CoverMediator: Failed to get currently playing from Spotify: {ex.Message}");
            }
            
            return null;
        }

        private async Task LoadSpotifyAlbumArtFromTrackAsync(SpotifyTrack track)
        {
            try
            {
                var albumImageUrl = track.AlbumImageUrl;
                if (!string.IsNullOrEmpty(albumImageUrl))
                {
                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(albumImageUrl);
                    
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = new MemoryStream(imageBytes);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                    
                    // Update UI on the main thread
                    if (Application.Current?.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            coverControl.CoverImageSource = image;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Spotify album art from track: {ex.Message}");
            }
        }

        private async Task LoadSpotifyAlbumArtAsync(SpotifyTrack track)
        {
            await LoadSpotifyAlbumArtFromTrackAsync(track);
        }

        private async Task UpdateSpotifyTrackInfo()
        {
            try
            {
                var currentlyPlaying = await GetCurrentlyPlayingFromSpotifyAsync();
                
                if (currentlyPlaying?.Item != null)
                {
                    // Check if this API data matches our expected track
                    var apiTrackUri = $"spotify:track:{currentlyPlaying.Item.Id}";
                    
                    // Only update if this matches the expected track OR if we don't have an expected track set
                    if (string.IsNullOrEmpty(currentExpectedTrackUri) || apiTrackUri == currentExpectedTrackUri)
                    {
                        // Update track info from real-time API on UI thread
                        if (Application.Current?.Dispatcher != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                coverControl.TitleText = currentlyPlaying.Item.Name;
                                coverControl.ArtistText = currentlyPlaying.Item.Artist;
                                coverControl.AlbumText = currentlyPlaying.Item.Album;
                                coverControl.YearText = !string.IsNullOrEmpty(currentlyPlaying.Item.AlbumYear) 
                                    ? currentlyPlaying.Item.AlbumYear 
                                    : "";
                            });
                        }
                        
                        // Load album art
                        if (!string.IsNullOrEmpty(currentlyPlaying.Item.AlbumImageUrl))
                        {
                            await LoadSpotifyAlbumArtFromTrackAsync(currentlyPlaying.Item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CoverMediator: Error updating Spotify track info: {ex.Message}");
            }
        }

        private Task HandleSpotifyTrackFromFileNode(FileNode fileNode)
        {
            try
            {
                // Try to parse track info from the display name
                var displayName = fileNode.DisplayName;
                if (!string.IsNullOrEmpty(displayName))
                {
                    // Set basic track info from display name
                    coverControl.TitleText = displayName;
                    
                    // Try to split artist and title if format is "Title - Artist" or "Artist - Title"
                    if (displayName.Contains(" - "))
                    {
                        var parts = displayName.Split(new[] { " - " }, 2, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            // Assume format is "Title - Artist"
                            coverControl.TitleText = parts[0].Trim();
                            coverControl.ArtistText = parts[1].Trim();
                        }
                    }
                    else
                    {
                        coverControl.ArtistText = "Spotify Track";
                    }
                    
                    coverControl.AlbumText = "Spotify";
                    coverControl.YearText = "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CoverMediator: Error in HandleSpotifyTrackFromFileNode: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }
    }
}
