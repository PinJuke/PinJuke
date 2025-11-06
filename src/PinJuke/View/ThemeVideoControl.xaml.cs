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
    public partial class ThemeVideoControl : BaseControl
    {
        public event EventHandler? StartMediaEndedEvent;
        public event EventHandler? StopMediaEndedEvent;
        public event EventHandler? LoopMediaEndedEvent;
        public event EventHandler? IdleMediaEndedEvent;

        private float contentRotation = 0f;
        public float ContentRotation
        {
            get => contentRotation;
            set => this.SetField(ref contentRotation, value);
        }

        public ThemeVideoControl()
        {
            InitializeComponent();

            // Behavior defaults are:
            //MediaElement.LoadedBehavior = MediaPlaybackState.Manual;
            //MediaElement.UnloadedBehavior = MediaPlaybackState.Close;
            //MediaElement.LoopingBehavior = MediaPlaybackState.Pause;

            StartMediaElement.LoadedBehavior = MediaPlaybackState.Stop;
            StopMediaElement.LoadedBehavior = MediaPlaybackState.Stop;
            LoopMediaElement.LoadedBehavior = MediaPlaybackState.Stop;
            LoopMediaElement.LoopingBehavior = MediaPlaybackState.Play;
            IdleMediaElement.LoadedBehavior = MediaPlaybackState.Stop;
            IdleMediaElement.LoopingBehavior = MediaPlaybackState.Play;

            StartMediaElement.Visibility = Visibility.Hidden;
            StopMediaElement.Visibility = Visibility.Hidden;
            LoopMediaElement.Visibility = Visibility.Hidden;
            IdleMediaElement.Visibility = Visibility.Hidden;

            StartMediaElement.MediaEnded += MediaElement_MediaEnded;
            StartMediaElement.MediaStateChanged += MediaElement_MediaStateChanged;
            StopMediaElement.MediaEnded += MediaElement_MediaEnded;
            StopMediaElement.MediaStateChanged += MediaElement_MediaStateChanged;
            LoopMediaElement.MediaEnded += MediaElement_MediaEnded;
            LoopMediaElement.MediaStateChanged += MediaElement_MediaStateChanged;
            IdleMediaElement.MediaEnded += MediaElement_MediaEnded;
            IdleMediaElement.MediaStateChanged += MediaElement_MediaStateChanged;
        }

        private void MediaElement_MediaStateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            var mediaElement = (MediaElement)sender!;
            Debug.WriteLine($"ThemeVideoControl: {mediaElement.Name}: media state: " + e.MediaState);
        }

        private void MediaElement_MediaEnded(object? sender, EventArgs e)
        {
            var mediaElement = (MediaElement)sender!;
            //if (mediaElement.MediaState == MediaPlaybackState.Stop)
            //{
            //    Debug.WriteLine($"ThemeVideoControl: {mediaElement.Name}: Media ended while stopped. Ignoring...");
            //    return;
            //}
            Debug.WriteLine($"ThemeVideoControl: {mediaElement.Name}: Media ended. Dispatching...");
            if (mediaElement == StartMediaElement)
            {
                StartMediaEndedEvent?.Invoke(this, EventArgs.Empty);
            }
            else if (mediaElement == StopMediaElement)
            {
                StopMediaEndedEvent?.Invoke(this, EventArgs.Empty);
            }
            else if (mediaElement == LoopMediaElement)
            {
                LoopMediaEndedEvent?.Invoke(this, EventArgs.Empty);
            }
            else if (mediaElement == IdleMediaElement)
            {
                IdleMediaEndedEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
