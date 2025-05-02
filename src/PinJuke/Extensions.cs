using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke
{
    public static class Extensions
    {
        private static readonly Random rng = new Random();

        /// <summary>
        /// See https://stackoverflow.com/a/1262619
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Redefine IsNullOrEmpty for PinJuke, because DOF ships their own version.
        /// </summary>
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static void Raise(this PropertyChangedEventHandler? handler, object sender, [CallerMemberName] string propertyName = "")
        {
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static bool SetField<T>(this IChangingProperties obj, ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            // https://stackoverflow.com/a/1316417
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            obj.NotifyPropertyChanged(propertyName);
            return true;
        }
    }
}
