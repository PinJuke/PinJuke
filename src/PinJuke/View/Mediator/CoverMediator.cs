﻿using DirectOutput.Cab.Out.DMX;
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
    public class CoverMediator : Mediator
    {
        private readonly CoverControl coverControl;
        private readonly MainModel mainModel;

        public CoverMediator(CoverControl coverControl, MainModel mainModel) : base(coverControl)
        {
            this.coverControl = coverControl;
            this.mainModel = mainModel;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateView();
            mainModel.PropertyChanged += MainModel_PropertyChanged;
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
                case nameof(MainModel.PlayingFile):
                    UpdateView();
                    break;
            }
        }

        private void UpdateView()
        {
            string? title = null;
            string? artist = null;
            string? year = null;
            string? album = null;
            BitmapImage? image = null;

            if (mainModel.PlayingFile != null)
            {
                TagLib.File? file = null;
                var path = mainModel.PlayingFile.FullName;
                try
                {
                    file = TagLib.File.Create(path);
                }
                catch (IOException ex)
                {
                }

                if (file != null)
                {
                    title = file.Tag.Title;
                    artist = string.Join(", ", file.Tag.Performers);
                    year = file.Tag.Year.ToString();
                    album = file.Tag.Album;

                    if (file.Tag.Pictures.Length > 0)
                    {
                        using (var stream = new MemoryStream(file.Tag.Pictures[0].Data.Data))
                        {
                            // https://stackoverflow.com/a/5346766
                            image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = stream;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.Freeze();
                        }
                    }
                }
            }

            coverControl.TitleText = title;
            coverControl.ArtistText = artist;
            coverControl.YearText = year;
            coverControl.AlbumText = album;
            coverControl.CoverImageSource = image;
        }
    }
}
