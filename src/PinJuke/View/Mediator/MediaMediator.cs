using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.View.Media;
using PinJuke.Spotify;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke.View.Mediator
{
    public class MediaMediator : Mediator
    {
        private readonly MediaControl mediaControl;
        private readonly MainModel mainModel;
        private readonly SpotifyMediaProvider? spotifyMediaProvider;

        private readonly MediaActionQueue mediaActionQueue;

        public MediaMediator(MediaControl mediaControl, MainModel mainModel, SpotifyMediaProvider? spotifyMediaProvider = null) : base(mediaControl)
        {
            this.mediaControl = mediaControl;
            this.mainModel = mainModel;
            this.spotifyMediaProvider = spotifyMediaProvider;

            mediaActionQueue = new(mediaControl.MediaElement);

            mediaControl.MediaEndedEvent += MediaControl_MediaEndedEvent; ;
        }

        private void MediaControl_MediaEndedEvent(object? sender, EventArgs e)
        {
            mainModel.MediaEnded();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            PlayFile();
            mainModel.PropertyChanged += MainModel_PropertyChanged;
        }

        protected override void OnUnloaded()
        {
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
            base.OnUnloaded();
        }

        private void MainModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.MediaPlayingFile):
                case nameof(MainModel.SceneType):
                    PlayFile();
                    break;
                case nameof(MainModel.MediaPlaying):
                    SetPlayPause();
                    break;
            }
        }

        private void PlayFile()
        {
            FileType? fileType = null;

            switch (mainModel.SceneType)
            {
                case SceneType.Intro:
                    var uri = new Uri(@"resources\intro.mp4", UriKind.Relative);
                    var stream = Application.GetResourceStream(uri).Stream;
                    mediaActionQueue.Open(new MediaInputStream(stream, uri));
                    mediaActionQueue.Play();
                    fileType = FileType.Video;
                    break;
                case SceneType.Playback:
                    if (mainModel.MediaPlayingFile != null)
                    {
                        // Check if this is a Spotify track
                        if (spotifyMediaProvider?.CanHandle(mainModel.MediaPlayingFile) == true)
                        {
                            // Handle Spotify tracks asynchronously
                            _ = PlaySpotifyTrackAsync(mainModel.MediaPlayingFile);
                        }
                        else
                        {
                            // Handle regular files
                            mediaActionQueue.Open(mainModel.MediaPlayingFile.FullName);
                            SetPlayPause();
                        }
                    }
                    else
                    {
                        mediaActionQueue.Close();
                    }
                    fileType = mainModel.MediaPlayingFile?.Type;
                    break;
            }

            mediaControl.MediaElement.Visibility = fileType == FileType.Video ? Visibility.Visible : Visibility.Hidden;
        }

        private async Task PlaySpotifyTrackAsync(FileNode spotifyTrack)
        {
            try
            {
                Debug.WriteLine($"MediaMediator.PlaySpotifyTrackAsync: Starting playback for {spotifyTrack.DisplayName}");
                
                if (spotifyMediaProvider == null)
                {
                    Debug.WriteLine("MediaMediator.PlaySpotifyTrackAsync: spotifyMediaProvider is null!");
                    return;
                }

                var result = await spotifyMediaProvider.CreateMediaStreamAsync(spotifyTrack);
                if (result == "spotify:external:playback")
                {
                    Debug.WriteLine($"MediaMediator.PlaySpotifyTrackAsync: Spotify external playback started for {spotifyTrack.DisplayName}");
                    
                    // Close any local media playback since Spotify is handling it externally
                    mediaActionQueue.Close();
                    
                    // The visualizations will pick up the audio from system audio capture
                    // No need to load anything into the media element
                }
                else
                {
                    Debug.WriteLine($"Spotify track not available for playback: {spotifyTrack.DisplayName}");
                    mediaActionQueue.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing Spotify track: {ex.Message}");
                mediaActionQueue.Close();
            }
        }

        private void SetPlayPause()
        {
            // Check if current track is a Spotify track
            if (mainModel.MediaPlayingFile != null && 
                spotifyMediaProvider?.CanHandle(mainModel.MediaPlayingFile) == true)
            {
                // Handle Spotify playback control
                _ = SetSpotifyPlayPauseAsync();
            }
            else
            {
                // Handle regular media files
                if (mainModel.MediaPlaying)
                {
                    mediaActionQueue.Play();
                }
                else
                {
                    mediaActionQueue.Pause();
                }
            }
        }

        private async Task SetSpotifyPlayPauseAsync()
        {
            try
            {
                if (spotifyMediaProvider == null) return;

                bool success;
                if (mainModel.MediaPlaying)
                {
                    Debug.WriteLine("MediaMediator: Resuming Spotify playback...");
                    success = await spotifyMediaProvider.ResumeAsync();
                }
                else
                {
                    Debug.WriteLine("MediaMediator: Pausing Spotify playback...");
                    success = await spotifyMediaProvider.PauseAsync();
                }

                if (!success)
                {
                    Debug.WriteLine($"MediaMediator: Failed to {(mainModel.MediaPlaying ? "resume" : "pause")} Spotify playback");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MediaMediator: Error controlling Spotify playback: {ex.Message}");
            }
        }

    }
}
