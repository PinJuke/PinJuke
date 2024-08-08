using DirectOutput.Cab.Out.DMX;
using PinJuke.Audio;
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
        private readonly AudioManager audioManager;

        public VisualizerMediator(VisualizerControl visualizerControl, MainModel mainModel, AudioManager audioManager) : base(visualizerControl)
        {
            this.visualizerControl = visualizerControl;
            this.mainModel = mainModel;
            this.audioManager = audioManager;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            UpdateView();
            mainModel.PropertyChanged += MainModel_PropertyChanged;

            visualizerControl.Initialize(audioManager, mainModel.Configuration.Milkdrop);
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
                case nameof(MainModel.MilkdropInfoVisible):
                    UpdateView();
                    break;
            }
        }

        private void UpdateView()
        {
            visualizerControl.MilkdropInfoVisible = mainModel.MilkdropInfoVisible;
        }
    }
}
