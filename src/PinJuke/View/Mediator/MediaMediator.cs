using DirectOutput.Cab.Out.DMX;
using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.View.Media;
using System;
using System.Collections.Generic;
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

        private readonly MediaActionQueue mediaActionQueue;

        public MediaMediator(MediaControl mediaControl, MainModel mainModel) : base(mediaControl)
        {
            this.mediaControl = mediaControl;
            this.mainModel = mainModel;

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
                case nameof(MainModel.PlayingFile):
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
                    if (mainModel.PlayingFile != null)
                    {
                        mediaActionQueue.Open(mainModel.PlayingFile.FullName);
                        SetPlayPause();
                    }
                    else
                    {
                        mediaActionQueue.Close();
                    }
                    fileType = mainModel.PlayingFile?.Type;
                    break;
            }

            mediaControl.MediaElement.Visibility = fileType == FileType.Video ? Visibility.Visible : Visibility.Hidden;
        }

        private void SetPlayPause()
        {
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
}
