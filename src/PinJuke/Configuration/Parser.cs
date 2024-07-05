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
            return s.IsNullOrEmpty();
        }

        public string FormatUndefined()
        {
            return "";
        }

        public string? ParseString(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            return s;
        }

        public string? FormatString(string? s)
        {
            return s == null ? FormatUndefined() : s;
        }

        public int? ParseInt(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            return int.TryParse(s, CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        public string? FormatInt(int? i)
        {
            return i == null ? FormatUndefined() : i.Value.ToString(CultureInfo.InvariantCulture);
        }

        public float? ParseFloat(string? s)
        {
            if (IsUndefined(s))
            {
                return null;
            }
            return float.TryParse(s, CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        public string? FormatFloat(float? f)
        {
            return f == null ? FormatUndefined() : f.Value.ToString(CultureInfo.InvariantCulture);
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

        public string? FormatBool(bool? b)
        {
            return b == null ? FormatUndefined() : FormatInt(b.Value ? 1 : 0);
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

        public string? FormatEnum<TEnum>(TEnum? e) where TEnum : struct
        {
            return e == null ? FormatUndefined() : e.Value.ToString();
        }

    }
}
