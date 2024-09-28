using DirectOutput.FX.TimmedFX;
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

        private string? openFileQueued = null;
        private IMediaInputStream? openMediaInputStreamQueued = null;
        private bool closeQueued = false;
        private MediaActionQueueState stateQueued = MediaActionQueueState.None;

        private bool running = false;
        
        public MediaActionQueue(Unosquare.FFME.MediaElement mediaElement, int delay = DELAY)
        {
            this.mediaElement = mediaElement;
            this.delay = delay;
        }

        public void Open(string file)
        {
            openFileQueued = file;
            openMediaInputStreamQueued?.Dispose();
            openMediaInputStreamQueued = null;
            closeQueued = false;
            stateQueued = MediaActionQueueState.None;
            Do();
        }

        public void Open(IMediaInputStream mediaInputStream)
        {
            openFileQueued = null;
            openMediaInputStreamQueued = mediaInputStream;
            closeQueued = false;
            stateQueued = MediaActionQueueState.None;
            Do();
        }

        public void Close()
        {
            openFileQueued = null;
            openMediaInputStreamQueued?.Dispose();
            openMediaInputStreamQueued = null;
            closeQueued = true;
            stateQueued = MediaActionQueueState.None;
            Do();
        }

        public void Play()
        {
            stateQueued = MediaActionQueueState.Play;
            Do();
        }

        public void Pause()
        {
            stateQueued = MediaActionQueueState.Pause;
            Do();
        }

        public void Stop()
        {
            stateQueued = MediaActionQueueState.Stop;
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
                if (openFileQueued != null)
                {
                    var file = openFileQueued;
                    openFileQueued = null;

                    await Unload();

                    Debug.WriteLine(string.Format("Opening file \"{0}\"...", file));
                    await mediaElement.Open(new Uri(file));
                    continue;
                }
                if (openMediaInputStreamQueued != null)
                {
                    var mediaInputStream = openMediaInputStreamQueued;
                    openMediaInputStreamQueued = null;

                    await Unload();

                    Debug.WriteLine("Opening media input stream...");
                    await mediaElement.Open(mediaInputStream);
                    continue;
                }
                if (closeQueued)
                {
                    closeQueued = false;
                    await Unload();
                    continue;
                }
                if (stateQueued == MediaActionQueueState.Play)
                {
                    stateQueued = MediaActionQueueState.None;
                    Debug.WriteLine("Playing...");
                    await mediaElement.Play();
                    continue;
                }
                if (stateQueued == MediaActionQueueState.Pause)
                {
                    stateQueued = MediaActionQueueState.None;
                    Debug.WriteLine("Pausing...");
                    await mediaElement.Pause();
                    continue;
                }
                if (stateQueued == MediaActionQueueState.Stop)
                {
                    stateQueued = MediaActionQueueState.None;
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
            // Still testing...
            // GC.Collect();
        }
    }
}
