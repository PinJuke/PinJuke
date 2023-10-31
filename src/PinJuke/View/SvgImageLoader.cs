using SVGImage.SVG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PinJuke.View
{
    // Warning CA1416
    // https://stackoverflow.com/a/70010939
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class SvgImageLoader
    {
        public static SvgImageLoader Instance { get; } = new SvgImageLoader();

        private readonly SVGRender svgRender = new();

        private readonly Dictionary<string, DrawingImage> images = new();

        public DrawingImage GetFromResource(string uriString)
        {
            if (images.TryGetValue(uriString, out var image))
            {
                return image;
            }

            return images[uriString] = LoadFromResource(uriString);
        }

        private DrawingImage LoadFromResource(string uriString)
        {
            var streamResourceInfo = Application.GetResourceStream(new Uri(uriString, UriKind.Relative));
            var drawing = svgRender.LoadDrawing(streamResourceInfo.Stream);
            var drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();
            return drawingImage;
        }

    }
}
