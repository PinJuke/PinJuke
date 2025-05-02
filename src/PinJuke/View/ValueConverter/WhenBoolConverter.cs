using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PinJuke.View.ValueConverter
{
    public class WhenBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string parameterString)
            {
                parameter = bool.Parse(parameterString);
            }
            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string parameterString)
            {
                parameter = bool.Parse(parameterString);
            }
            return ((bool)value) ? parameter : Binding.DoNothing;
        }
    }
}
