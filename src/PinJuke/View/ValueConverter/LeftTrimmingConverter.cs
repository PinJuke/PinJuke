using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PinJuke.View.ValueConverter
{
    /// <summary>
    /// Some ideas taken from:
    /// https://stackoverflow.com/a/68934282/628696
    /// </summary>
    class LeftTrimmer
    {
        private const string Ellipsis = "…";

        private readonly TextBlock reference;
        private readonly string text;

        private readonly Typeface typeface;
        private readonly double fontSize;
        private readonly FlowDirection flowDirection;
        private readonly double pixelsPerDip;
        private readonly CultureInfo cultureInfo;

        public LeftTrimmer(TextBlock reference, string text)
        {
            this.reference = reference;
            this.text = text;

            typeface = new Typeface(reference.FontFamily, reference.FontStyle, reference.FontWeight, reference.FontStretch);
            fontSize = reference.FontSize;
            flowDirection = reference.FlowDirection;

            // https://stackoverflow.com/a/58344175
            var dpiInfo = VisualTreeHelper.GetDpi(reference);
            pixelsPerDip = dpiInfo.PixelsPerDip;

            cultureInfo = CultureInfo.CurrentCulture;
        }

        public string GetTrimmedText()
        {
            var maxWidth = reference.ActualWidth - reference.Padding.Left - reference.Padding.Right;
            if (MeasureString(text).Width <= maxWidth)
            {
                return text;
            }

            double ellipsisWidth = MeasureString(Ellipsis).Width;
            var clippedText = ClipTextToWidth(text, maxWidth - ellipsisWidth);
            // For RTL force the ellipsis on the left side.
            return flowDirection == FlowDirection.RightToLeft ? clippedText + Ellipsis : Ellipsis + clippedText;
        }

        private string ClipTextToWidth(string text, double maxWidth)
        {
            int start = (int)Math.Ceiling(text.Length / 2.0f);
            var half = text.Substring(start, text.Length / 2);
            if (half.Length == 0)
            {
                return "";
            }
            double actualWidth = MeasureString(half).Width;
            if (actualWidth > maxWidth)
            {
                return ClipTextToWidth(half, maxWidth);
            }
            return ClipTextToWidth(text.Substring(0, start), maxWidth - actualWidth) + half;
        }

        private Size MeasureString(string candidate)
        {
            // FormattedText must be created each time...
            var formattedText = new FormattedText(
                candidate,
                cultureInfo,
                flowDirection,
                typeface,
                fontSize,
                Brushes.Black,
                pixelsPerDip
            );
            return new Size(formattedText.Width, formattedText.Height);
        }
    }

    public class LeftTrimmingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
            {
                return "";
            }
            var text = values[0]?.ToString();
            var reference = values[1] as TextBlock;
            if (string.IsNullOrEmpty(text) || reference == null)
            {
                return "";
            }
            return new LeftTrimmer(reference, text).GetTrimmedText();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            CultureInfo culture) => throw new NotImplementedException();
    }
}
