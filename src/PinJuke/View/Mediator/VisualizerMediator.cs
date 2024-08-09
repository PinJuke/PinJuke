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
            }
        }

        private void MainModel_PresetEvent(object? sender, PresetActionEventArgs e)
        {
            Play(e.PresetAction);
        }

        private async void Play(PresetAction presetAction)
        {
            // TODO: Does this help if more than one visualizer is displayed?
            await Task.Delay((int)displayConfig.Role * 2000);

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
    }
}
