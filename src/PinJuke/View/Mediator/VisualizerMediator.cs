using DirectOutput.Cab.Out.DMX;
using PinJuke.Audio;
using PinJuke.Configuration;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PinJuke.View.Mediator
{
    public class VisualizerMediator : Mediator
    {
        private readonly VisualizerControl visualizerControl;
        private readonly MainModel mainModel;
        private readonly Display displayConfig;
        private readonly AudioManager audioManager;

        public VisualizerMediator(VisualizerControl visualizerControl, MainModel mainModel, Display displayConfig, AudioManager audioManager) : base(visualizerControl)
        {
            this.visualizerControl = visualizerControl;
            this.mainModel = mainModel;
            this.displayConfig = displayConfig;
            this.audioManager = audioManager;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            UpdateView();
            UpdateVisibility();
            mainModel.PropertyChanged += MainModel_PropertyChanged;
            mainModel.PresetEvent += MainModel_PresetEvent;

            visualizerControl.Initialize(audioManager, mainModel.Configuration.Milkdrop);
        }

        protected override void OnUnloaded()
        {
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
            mainModel.PresetEvent -= MainModel_PresetEvent;
            base.OnUnloaded();
        }

        private void MainModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.PresetInfoVisible):
                    UpdateView();
                    break;
                case nameof(MainModel.MediaPlaying):
                    UpdateVisibility();
                    break;
            }
        }

        private void MainModel_PresetEvent(object? sender, PresetActionEventArgs e)
        {
            Play(e.PresetAction);
        }

        private void Play(PresetAction presetAction)
        {
            switch (presetAction)
            {
                case PresetAction.Next:
                    visualizerControl.PlayNext();
                    break;
                case PresetAction.Previous:
                    visualizerControl.PlayPrevious();
                    break;
            }
        }

        private void UpdateView()
        {
            visualizerControl.PresetInfoVisible = mainModel.PresetInfoVisible;
        }

        private void UpdateVisibility()
        {
            var visible = mainModel.MediaPlaying
                ? displayConfig.Content.PlaybackBackgroundType == BackgroundType.MilkdropVisualization
                : displayConfig.Content.IdleBackgroundType == BackgroundType.MilkdropVisualization;
            visualizerControl.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
