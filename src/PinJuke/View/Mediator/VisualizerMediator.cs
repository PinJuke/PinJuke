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

            visualizerControl.Initialize(audioManager, mainModel.Configuration.Milkdrop);
        }
    }
}
