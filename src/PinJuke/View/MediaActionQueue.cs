using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.FFME.Common;

namespace PinJuke.View
{
    public class MediaActionQueue
    {
        private const int DELAY = 100; // millis

        private readonly Unosquare.FFME.MediaElement mediaElement;

        private string? openFileQueued = null;
        private IMediaInputStream? openMediaInputStreamQueued = null;
        private bool closeQueued = false;
        private bool playQueued = false;
        private bool pauseQueued = false;

        private bool running = false;
        
        public MediaActionQueue(Unosquare.FFME.MediaElement mediaElement)
        {
            this.mediaElement = mediaElement;
        }

        public void Open(string file)
        {
            openFileQueued = file;
            openMediaInputStreamQueued?.Dispose();
            openMediaInputStreamQueued = null;
            closeQueued = false;
            playQueued = false;
            pauseQueued = false;
            Do();
        }

        public void Open(IMediaInputStream mediaInputStream)
        {
            openFileQueued = null;
            openMediaInputStreamQueued = mediaInputStream;
            closeQueued = false;
            playQueued = false;
            pauseQueued = false;
            Do();
        }

        public void Close()
        {
            closeQueued = true;
            openFileQueued = null;
            openMediaInputStreamQueued?.Dispose();
            openMediaInputStreamQueued = null;
            playQueued = false;
            pauseQueued = false;
            Do();
        }

        public void Play()
        {
            playQueued = true;
            pauseQueued = false;
            Do();
        }

        public void Pause()
        {
            pauseQueued = true;
            playQueued = false;
            Do();
        }

        private async void Do()
        {
            if (running)
            {
                return;
            }

            running = true;

            for (; ; await Task.Delay(DELAY))
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
                if (playQueued)
                {
                    playQueued = false;
                    Debug.WriteLine("Playing...");
                    await mediaElement.Play();
                    continue;
                }
                if (pauseQueued)
                {
                    pauseQueued = false;
                    Debug.WriteLine("Pausing...");
                    await mediaElement.Pause();
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
                await Task.Delay(DELAY);
            }
            if (mediaElement.IsOpen)
            {
                Debug.WriteLine("Closing...");
                await mediaElement.Close();
                await Task.Delay(DELAY);
            }
            // Still testing...
            GC.Collect();
        }
    }
}
