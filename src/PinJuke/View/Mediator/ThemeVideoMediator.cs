using DirectOutput.Cab.Out.DMX;
using PinJuke.Configuration;
using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.View.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unosquare.FFME.Common;

namespace PinJuke.View.Mediator
{
    public class ThemeVideoMediator : Mediator
    {
        private readonly ThemeVideoControl themeVideoControl;
        private readonly MainModel mainModel;
        private readonly Display displayConfig;

        private MediaInputStream? startMediaInputStream = null;
        private MediaInputStream? stopMediaInputStream = null;
        private MediaInputStream? loopMediaInputStream = null;
        private MediaInputStream? idleMediaInputStream = null;

        private readonly MediaActionQueue startMediaActionQueue;
        private readonly MediaActionQueue stopMediaActionQueue;
        private readonly MediaActionQueue loopMediaActionQueue;
        private readonly MediaActionQueue idleMediaActionQueue;

        private MediaEventToken<MediaEventData>? mediaEventToken = null;

        private bool init = false;

        public ThemeVideoMediator(ThemeVideoControl themeVideoControl, MainModel mainModel, Display displayConfig) : base(themeVideoControl)
        {
            this.themeVideoControl = themeVideoControl;
            this.mainModel = mainModel;
            this.displayConfig = displayConfig;

            var content = displayConfig.Content;

            themeVideoControl.ContentRotation = content.ThemeVideoRotation;

            startMediaActionQueue = new(themeVideoControl.StartMediaElement, 0);
            stopMediaActionQueue = new(themeVideoControl.StopMediaElement, 0);
            loopMediaActionQueue = new(themeVideoControl.LoopMediaElement, 0);
            idleMediaActionQueue = new(themeVideoControl.IdleMediaElement, 0);

            if (content.ThemeVideoStartFileEnabled)
            {
                startMediaInputStream = CreateMediaInputStream(content.ThemeVideoStartFile);
                if (startMediaInputStream != null)
                {
                    startMediaActionQueue.Open(startMediaInputStream);
                }
            }
            if (content.ThemeVideoStopFileEnabled)
            {
                stopMediaInputStream = CreateMediaInputStream(content.ThemeVideoStopFile);
                if (stopMediaInputStream != null)
                {
                    stopMediaActionQueue.Open(stopMediaInputStream);
                }
            }
            if (content.PlaybackBackgroundType == BackgroundType.Video)
            {
                loopMediaInputStream = CreateMediaInputStream(content.ThemeVideoLoopFile);
                if (loopMediaInputStream != null)
                {
                    loopMediaActionQueue.Open(loopMediaInputStream);
                }
            }
            if (content.IdleBackgroundType == BackgroundType.Video)
            {
                idleMediaInputStream = CreateMediaInputStream(content.ThemeVideoIdleFile);
                if (idleMediaInputStream != null)
                {
                    idleMediaActionQueue.Open(idleMediaInputStream);
                }
            }

            themeVideoControl.StartMediaEndedEvent += ThemeVideoControl_StartMediaEndedEvent;
            themeVideoControl.StopMediaEndedEvent += ThemeVideoControl_StopMediaEndedEvent;
            themeVideoControl.LoopMediaEndedEvent += ThemeVideoControl_LoopMediaEndedEvent;
            themeVideoControl.IdleMediaEndedEvent += ThemeVideoControl_IdleMediaEndedEvent;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            mainModel.MediaEvent += MainModel_MediaEvent;
        }

        protected override void OnUnloaded()
        {
            mainModel.MediaEvent -= MainModel_MediaEvent;
            base.OnUnloaded();
        }

        private MediaInputStream? CreateMediaInputStream(string filePath)
        {
            if (filePath.IsNullOrEmpty())
            {
                return null;
            }
            var memoryStream = new MemoryStream();
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                }
            }
            catch (IOException)
            {
                Debug.WriteLine($"Error reading \"{filePath}\".");
                return null;
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new(memoryStream, new Uri(filePath), true);
        }

        private void MainModel_MediaEvent(object? sender, MediaEventArgs<MediaEventData> e)
        {
            switch (e.Type)
            {
                case MediaEventType.Play:
                    OnPlay(e);
                    break;
                case MediaEventType.End:
                    OnEnd(e);
                    break;
            }
        }

        private void OnPlay(MediaEventArgs<MediaEventData> e)
        {
            if (init && idleMediaInputStream != null)
            {
                mediaEventToken = e.Intercept();
                return;
            }
            init = true;
            if (PlayOrLoop())
            {
                mediaEventToken = e.Intercept();
            }
        }

        private void OnEnd(MediaEventArgs<MediaEventData> e)
        {
            if (init && loopMediaInputStream != null)
            {
                mediaEventToken = e.Intercept();
                return;
            }
            init = true;
            if (StopOrIdle())
            {
                mediaEventToken = e.Intercept();
            }
        }

        private void ThemeVideoControl_StartMediaEndedEvent(object? sender, EventArgs e)
        {
            themeVideoControl.StartMediaElement.Visibility = Visibility.Hidden;
            startMediaActionQueue.Stop();
            if (loopMediaInputStream != null)
            {
                loopMediaActionQueue.Play();
                themeVideoControl.LoopMediaElement.Visibility = Visibility.Visible;
            }
            ContinueMediaEventToken();
        }

        private void ThemeVideoControl_StopMediaEndedEvent(object? sender, EventArgs e)
        {
            themeVideoControl.StopMediaElement.Visibility = Visibility.Hidden;
            stopMediaActionQueue.Stop();
            if (idleMediaInputStream != null)
            {
                idleMediaActionQueue.Play();
                themeVideoControl.IdleMediaElement.Visibility = Visibility.Visible;
            }
            ContinueMediaEventToken();
        }

        private void ThemeVideoControl_LoopMediaEndedEvent(object? sender, EventArgs e)
        {
            if (mediaEventToken != null)
            {
                themeVideoControl.LoopMediaElement.Visibility = Visibility.Hidden;
                loopMediaActionQueue.Stop();

                StopOrIdle();
            }
        }

        private void ThemeVideoControl_IdleMediaEndedEvent(object? sender, EventArgs e)
        {
            if (mediaEventToken != null)
            {
                themeVideoControl.IdleMediaElement.Visibility = Visibility.Hidden;
                idleMediaActionQueue.Stop();

                PlayOrLoop();
            }
        }

        private bool PlayOrLoop()
        {
            if (startMediaInputStream != null)
            {
                startMediaActionQueue.Play();
                themeVideoControl.StartMediaElement.Visibility = Visibility.Visible;
                return true;
            }
            if (loopMediaInputStream != null)
            {
                loopMediaActionQueue.Play();
                themeVideoControl.LoopMediaElement.Visibility = Visibility.Visible;
            }
            ContinueMediaEventToken();
            return false;
        }

        private bool StopOrIdle()
        {
            if (stopMediaInputStream != null)
            {
                stopMediaActionQueue.Play();
                themeVideoControl.StopMediaElement.Visibility = Visibility.Visible;
                return true;
            }
            if (idleMediaInputStream != null)
            {
                idleMediaActionQueue.Play();
                themeVideoControl.IdleMediaElement.Visibility = Visibility.Visible;
            }
            ContinueMediaEventToken();
            return false;
        }

        private void ContinueMediaEventToken()
        {
            var token = this.mediaEventToken;
            this.mediaEventToken = null;
            token?.Continue();
        }
    }
}
