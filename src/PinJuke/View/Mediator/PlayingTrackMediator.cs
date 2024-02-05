using DirectOutput.Cab.Out.DMX;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PinJuke.View.Mediator
{
    public class PlayingTrackMediator : Mediator
    {
        private readonly PlayingTrackControl playingTrackControl;
        private readonly MainModel mainModel;

        public PlayingTrackMediator(PlayingTrackControl playingTrackControl, MainModel mainModel) : base(playingTrackControl)
        {
            this.playingTrackControl = playingTrackControl;
            this.mainModel = mainModel;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateView();
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
                    break;
                case nameof(MainModel.Playing):
                    UpdateView();
                    break;
                case nameof(MainModel.PlayingTrackVisible):
                    UpdateView();
                    break;
            }
        }

        private void UpdateView()
        {
            playingTrackControl.FileNode = mainModel.PlayingFile;
            playingTrackControl.Playing = mainModel.Playing;
            playingTrackControl.ViewVisible = mainModel.PlayingTrackVisible;
        }
    }
}
