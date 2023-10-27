using DotNetProjects.SVGImage.SVG.FileLoaders;
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
    public class SvgImageLoader
    {
        public static SvgImageLoader Instance { get; } = new SvgImageLoader();

        private readonly SVGRender svgRender = new SVGRender(new FileSystemLoader());

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
            //drawing.Freeze();
            var drawingImage = new DrawingImage(drawing);
            //drawingImage.Freeze();
            return drawingImage;
        }

    }
}
