using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unosquare.FFME;
using Unosquare.FFME.Common;

namespace PinJuke.View
{
    public partial class MediaControl : BaseControl
    {
        public event EventHandler? MediaEndedEvent;

        private bool closed = true;

        public MediaControl()
        {
            InitializeComponent();

            // https://github.com/unosquare/ffmediaelement/issues/388#issuecomment-491851750
            MediaElement.RendererOptions.UseLegacyAudioOut = true;
            // Behavior defaults are:
            //MediaElement.LoadedBehavior = MediaPlaybackState.Manual;
            //MediaElement.UnloadedBehavior = MediaPlaybackState.Close;
            //MediaElement.LoopingBehavior = MediaPlaybackState.Pause;
            MediaElement.MediaEnded += MediaElement_MediaEnded;
            MediaElement.MediaStateChanged += MediaElement_MediaStateChanged;
        }

        private void MediaElement_MediaStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            Debug.WriteLine("MediaElement: media state: " + e.MediaState);
            switch (e.MediaState)
            {
                case MediaPlaybackState.Play:
                    closed = false;
                    break;
                case MediaPlaybackState.Close:
                    closed = true;
                    break;
            }
        }

        private void MediaElement_MediaEnded(object? sender, EventArgs e)
        {
            if (this.closed)
            {
                Debug.WriteLine("MediaElement: Media ended while closed. Ignoring...");
                return;
            }

            Debug.WriteLine("MediaElement: Media ended. Dispatching...");
            MediaEndedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
