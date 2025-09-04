using DirectOutput.Cab.Out.DMX;
using PinJuke.Model;
using PinJuke.Playlist;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PinJuke.View.Mediator
{
    public class PlayingTrackMediator : Mediator
    {
        private static readonly Dictionary<StateType, string> iconPaths = new()
        {
            {StateType.Previous, @"icons\play-skip-back-circle-outline.svg"},
            {StateType.Next, @"icons\play-skip-forward-circle-outline.svg"},
            {StateType.Play, @"icons\play-circle-outline.svg"},
            {StateType.Pause, @"icons\pause-circle-outline.svg"},
            {StateType.Stop, @"icons\stop-circle-outline.svg"},
        };

        private static readonly Dictionary<StateType, string> texts = new()
        {
            {StateType.Previous, Strings.StatePrevious},
            {StateType.Next, Strings.StateNext},
            {StateType.Play, Strings.StatePlay},
            {StateType.Pause, Strings.StatePause},
            {StateType.Stop, Strings.StateStop},
            {StateType.Tilt, Strings.StateTilt},
        };

        private static readonly string[] volumeIconPaths = {
            @"icons\volume-off-outline.svg",
            @"icons\volume-low-outline.svg",
            @"icons\volume-medium-outline.svg",
            @"icons\volume-high-outline.svg",
        };

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
                case nameof(MainModel.LastState):
                    UpdateView();
                    break;
                case nameof(MainModel.StateVisible):
                    UpdateView();
                    break;
            }
        }

        private void UpdateView()
        {
            var state = mainModel.LastState;
            string? iconPath = null;
            string? text = null;
            var stateVisible = mainModel.StateVisible;

            iconPaths.TryGetValue(state.Type, out iconPath);
            texts.TryGetValue(state.Type, out text);

            switch (state.Type)
            {
                case StateType.Volume:
                    var volumeLevel = (float?)state.Data;
                    text = volumeLevel == null ? Strings.NoAudioDeviceFound : string.Format(Strings.StateVolumeXPercent, volumeLevel * 100);
                    if (volumeLevel != null)
                    {
                        int i = Math.Max(0, Math.Min(volumeIconPaths.Length - 1, (int)(volumeLevel * volumeIconPaths.Length)));
                        iconPath = volumeIconPaths[i];
                    }
                    break;
                case StateType.Tilt:
                    stateVisible = false; // Hide display for now.
                    break;
            }

            playingTrackControl.StateImageSource = iconPath == null ? null : SvgImageLoader.Instance.GetFromResource(iconPath);
            playingTrackControl.StateText = text;
            playingTrackControl.ViewVisible = stateVisible;
        }
    }
}
