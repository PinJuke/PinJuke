using DirectOutput.Cab.Out.DMX;
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
    public class BackgroundImageMediator : Mediator
    {
        private readonly BackgroundImageControl backgroundImageControl;
        private readonly Configuration.Display displayConfig;

        public BackgroundImageMediator(BackgroundImageControl backgroundImageControl, Configuration.Display displayConfig) : base(backgroundImageControl)
        {
            this.backgroundImageControl = backgroundImageControl;
            this.displayConfig = displayConfig;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

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
    }
}
