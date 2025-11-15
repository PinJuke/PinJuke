﻿using DirectOutput.Cab.Out.DMX;
using PinJuke.Configuration;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PinJuke.View.Mediator
{
    public class BackgroundImageMediator : Mediator
    {
        private readonly BackgroundImageControl backgroundImageControl;
        private readonly MainModel mainModel;
        private readonly Configuration.Display displayConfig;

        public BackgroundImageMediator(BackgroundImageControl backgroundImageControl, MainModel mainModel, Configuration.Display displayConfig) : base(backgroundImageControl)
        {
            this.backgroundImageControl = backgroundImageControl;
            this.mainModel = mainModel;
            this.displayConfig = displayConfig;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateVisibility();
            mainModel.PropertyChanged += MainModel_PropertyChanged;
            SetupImage();
        }

        protected override void OnUnloaded()
        {
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
            base.OnUnloaded();
        }

        private void SetupImage()
        {
            var backgroundImageFile = displayConfig.Content.BackgroundImageFile;
            if (!backgroundImageFile.IsNullOrEmpty())
            {
                try
                {
                    backgroundImageControl.BackgroundImageSource = new BitmapImage(new Uri(backgroundImageFile));
                }
                catch (IOException)
                {
                    backgroundImageControl.ErrorMessage = string.Format(Strings.ErrorReadingFile, backgroundImageFile);
                }
            }
        }

        private void MainModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.MediaPlaying):
                    UpdateVisibility();
                    break;
            }
        }

        private void UpdateVisibility()
        {
            var visible = mainModel.MediaPlaying
                ? displayConfig.Content.PlaybackBackgroundType == BackgroundType.Image
                : displayConfig.Content.IdleBackgroundType == BackgroundType.Image;
            backgroundImageControl.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
