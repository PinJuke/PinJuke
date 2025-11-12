#if !DISABLE_DIRECTOUTPUT
using DirectOutput.FX.TimmedFX;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.FFME.Common;

namespace PinJuke.View
{
    public enum MediaActionQueueState
    {
        None,
        Play,
        Pause,
        Stop,
    }

    public class MediaActionQueue
    {
        private const int DELAY = 100; // millis

        private readonly Unosquare.FFME.MediaElement mediaElement;
        private readonly int delay;

        private Uri? openedUri = null;
        private IMediaInputStream? openedMediaInputStream = null;

        private string? queuedFile = null;
        private IMediaInputStream? queuedMediaInputStream = null;
        private bool queuedClose = false;
        private MediaActionQueueState queuedState = MediaActionQueueState.None;

        private bool running = false;
        
        public MediaActionQueue(Unosquare.FFME.MediaElement mediaElement, int delay = DELAY)
        {
            this.mediaElement = mediaElement;
            this.delay = delay;
        }

        public void Open(string file)
        {
            queuedFile = file;
            queuedMediaInputStream?.Dispose();
            queuedMediaInputStream = null;
            queuedClose = false;
            queuedState = MediaActionQueueState.None;
            Do();
        }

        public void Open(IMediaInputStream mediaInputStream)
        {
            queuedFile = null;
            queuedMediaInputStream = mediaInputStream;
            queuedClose = false;
            queuedState = MediaActionQueueState.None;
            Do();
        }

        public void Close()
        {
            queuedFile = null;
            queuedMediaInputStream?.Dispose();
            queuedMediaInputStream = null;
            queuedClose = true;
            queuedState = MediaActionQueueState.None;
            Do();
        }

        public void Play()
        {
            queuedState = MediaActionQueueState.Play;
            Do();
        }

        public void Pause()
        {
            queuedState = MediaActionQueueState.Pause;
            Do();
        }

        public void Stop()
        {
            queuedState = MediaActionQueueState.Stop;
            Do();
        }

        private async void Do()
        {
            if (running)
            {
                return;
            }

            running = true;

            for (; ; await Task.Delay(delay))
            {
                if (queuedFile != null)
                {
                    var file = queuedFile;
                    queuedFile = null;

                    await Unload();

                    Debug.WriteLine(string.Format("Opening file \"{0}\"...", file));
                    openedUri = new Uri(file);
                    await mediaElement.Open(openedUri);
                    continue;
                }
                if (queuedMediaInputStream != null)
                {
                    var mediaInputStream = queuedMediaInputStream;
                    queuedMediaInputStream = null;

                    await Unload();

                    Debug.WriteLine("Opening media input stream...");
                    openedMediaInputStream = mediaInputStream;
                    await mediaElement.Open(openedMediaInputStream);
                    continue;
                }
                if (queuedClose)
                {
                    queuedClose = false;
                    await Unload();
                    continue;
                }
                if (queuedState == MediaActionQueueState.Play)
                {
                    queuedState = MediaActionQueueState.None;
                    Debug.WriteLine("Playing...");
                    await mediaElement.Play();
                    continue;
                }
                if (queuedState == MediaActionQueueState.Pause)
                {
                    queuedState = MediaActionQueueState.None;
                    Debug.WriteLine("Pausing...");
                    await mediaElement.Pause();
                    continue;
                }
                if (queuedState == MediaActionQueueState.Stop)
                {
                    queuedState = MediaActionQueueState.None;
                    Debug.WriteLine("Stopping...");
                    await mediaElement.Stop();
                    continue;
                }
                break;
            }

            running = false;
        }
        
        private async Task Unload()
        {
            if (mediaElement.IsPlaying)
            {
                Debug.WriteLine("Stopping...");
                await mediaElement.Stop();
                await Task.Delay(delay);
            }
            if (mediaElement.IsOpen)
            {
                Debug.WriteLine("Closing...");
                await mediaElement.Close();
                await Task.Delay(delay);
            }
            openedUri = null;
            openedMediaInputStream?.Dispose();
            openedMediaInputStream = null;
            // Still testing...
            // GC.Collect();
        }
    }
}
