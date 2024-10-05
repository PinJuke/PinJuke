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
        private MediaInputStream? loopMediaInputStream = null;
        private MediaInputStream? stopMediaInputStream = null;

        private readonly MediaActionQueue startMediaActionQueue;
        private readonly MediaActionQueue loopMediaActionQueue;
        private readonly MediaActionQueue stopMediaActionQueue;

        private MediaEventToken? playMediaEventToken = null;
        private MediaEventToken? endMediaEventToken = null;

        private bool playing = false;

        public ThemeVideoMediator(ThemeVideoControl themeVideoControl, MainModel mainModel, Display displayConfig) : base(themeVideoControl)
        {
            this.themeVideoControl = themeVideoControl;
            this.mainModel = mainModel;
            this.displayConfig = displayConfig;

            themeVideoControl.ContentRotation = displayConfig.Content.ThemeVideoRotation;

            startMediaActionQueue = new(themeVideoControl.StartMediaElement, 0);
            loopMediaActionQueue = new(themeVideoControl.LoopMediaElement, 0);
            stopMediaActionQueue = new(themeVideoControl.StopMediaElement, 0);
            
            startMediaInputStream = CreateMediaInputStream(displayConfig.Content.ThemeVideoStartFile);
            if (startMediaInputStream != null)
            {
                startMediaActionQueue.Open(startMediaInputStream);
            }
            loopMediaInputStream = CreateMediaInputStream(displayConfig.Content.ThemeVideoLoopFile);
            if (loopMediaInputStream != null)
            {
                loopMediaActionQueue.Open(loopMediaInputStream);
            }
            stopMediaInputStream = CreateMediaInputStream(displayConfig.Content.ThemeVideoStopFile);
            if (stopMediaInputStream != null)
            {
                stopMediaActionQueue.Open(stopMediaInputStream);
            }

            themeVideoControl.StartMediaEndedEvent += ThemeVideoControl_StartMediaEndedEvent;
            themeVideoControl.LoopMediaEndedEvent += ThemeVideoControl_LoopMediaEndedEvent;
            themeVideoControl.StopMediaEndedEvent += ThemeVideoControl_StopMediaEndedEvent;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            mainModel.PlayMediaEvent += MainModel_PlayMediaEvent;
            mainModel.EndMediaEvent += MainModel_EndMediaEvent;
        }

        protected override void OnUnloaded()
        {
            mainModel.PlayMediaEvent -= MainModel_PlayMediaEvent;
            mainModel.EndMediaEvent -= MainModel_EndMediaEvent;
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
            catch (IOException ex)
            {
                Debug.WriteLine($"Error reading \"{filePath}\".");
                return null;
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new(memoryStream, new Uri(displayConfig.Content.ThemeVideoLoopFile), true);
        }

        private void MainModel_PlayMediaEvent(object? sender, PlayMediaEventArgs e)
        {
            if (playing && stopMediaInputStream != null)
            {
                playMediaEventToken = e.Intercept();
                stopMediaActionQueue.Play();
                themeVideoControl.StopMediaElement.Visibility = Visibility.Visible;
                return;
            }
            playing = mainModel.Playing;
            if (!playing)
            {
                return;
            }
            if (startMediaInputStream != null)
            {
                playMediaEventToken = e.Intercept();
                startMediaActionQueue.Play();
                themeVideoControl.StartMediaElement.Visibility = Visibility.Visible;
                return;
            }
            if (loopMediaInputStream != null)
            {
                loopMediaActionQueue.Play();
                themeVideoControl.LoopMediaElement.Visibility = Visibility.Visible;
            }
        }

        private void ThemeVideoControl_StopMediaEndedEvent(object? sender, EventArgs e)
        {
            themeVideoControl.StopMediaElement.Visibility = Visibility.Hidden;
            stopMediaActionQueue.Stop();
            playing = mainModel.Playing;
            if (!playing)
            {
                playMediaEventToken?.Continue();
                playMediaEventToken = null;
                return;
            }
            if (startMediaInputStream != null)
            {
                startMediaActionQueue.Play();
                themeVideoControl.StartMediaElement.Visibility = Visibility.Visible;
                return;
            }
            playMediaEventToken?.Continue();
            playMediaEventToken = null;
            if (loopMediaInputStream != null)
            {
                loopMediaActionQueue.Play();
                themeVideoControl.LoopMediaElement.Visibility = Visibility.Visible;
            }
        }

        private void ThemeVideoControl_StartMediaEndedEvent(object? sender, EventArgs e)
        {
            themeVideoControl.StartMediaElement.Visibility = Visibility.Hidden;
            startMediaActionQueue.Stop();

            playing = mainModel.Playing;
            if (!playing)
            {
                // Stopped while starting to play...
                stopMediaActionQueue.Play();
                themeVideoControl.StopMediaElement.Visibility = Visibility.Visible;
                return;
            }

            playMediaEventToken?.Continue();
            playMediaEventToken = null;
            if (loopMediaInputStream != null)
            {
                loopMediaActionQueue.Play();
                themeVideoControl.LoopMediaElement.Visibility = Visibility.Visible;
            }
        }

        private void MainModel_EndMediaEvent(object? sender, EndMediaEventArgs e)
        {
            if (!playing)
            {
                return;
            }
            if (loopMediaInputStream != null)
            {
                endMediaEventToken = e.Intercept();
            }
        }

        private void ThemeVideoControl_LoopMediaEndedEvent(object? sender, EventArgs e)
        {
            if (endMediaEventToken != null)
            {
                loopMediaActionQueue.Stop();
                themeVideoControl.LoopMediaElement.Visibility = Visibility.Hidden;
                loopMediaActionQueue.Stop();

                endMediaEventToken?.Continue();
                endMediaEventToken = null;
            }
        }
    }
}
