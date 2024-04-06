using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Configuration
{
    public class Parser
    {
        /// <summary>
        /// Should at least check for null.
        /// </summary>
        public bool IsUndefined(string? s)
        {
            return s == null;
        }

        public string? ParseString(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            return s;
        }

        public int? ParseInt(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return null;
        }

        public float? ParseFloat(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return null;
        }

        public bool? ParseBool(string? s)
        {
            var i = ParseInt(s);
            if (i == null)
            {
                return null;
            }
            return i != 0;
        }

        public TEnum? ParseEnum<TEnum>(string? s) where TEnum : struct
        {
            if (IsUndefined(s))
            {
                return null;
            }
            if (Enum.TryParse<TEnum>(s, out var result))
            {
                return result;
            }
            return null;
        }

    }
}
