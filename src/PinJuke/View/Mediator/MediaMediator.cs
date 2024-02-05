using DirectOutput.Cab.Out.DMX;
using PinJuke.Model;
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
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateView();
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
                    UpdateView();
                    PlayFile();
                    break;
                case nameof(MainModel.Playing):
                    SetPlayPause();
                    break;
            }
        }

        private void PlayFile()
        {
            if (mainModel.PlayingFile != null)
            {
                mediaActionQueue.Open(mainModel.PlayingFile.FullName);
                SetPlayPause();
            }
            else
            {
                mediaActionQueue.Close();
            }
        }

        private void SetPlayPause()
        {
            if (mainModel.Playing)
            {
                mediaActionQueue.Play();
            }
            else
            {
                mediaActionQueue.Pause();
            }
        }

        private void UpdateView()
        {
            var fileType = mainModel.PlayingFile?.Type;
            mediaControl.MediaElement.Visibility = fileType == Playlist.FileType.Video ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
