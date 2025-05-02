using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PinJuke.View.ValueConverter
{
    [ValueConversion(typeof(int), typeof(string))]
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue = (int)value;
            return System.Convert.ToString(intValue) ?? "";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = (string)value;
            return strValue == "" ? null : System.Convert.ToInt32(strValue);
        }
    }
}
