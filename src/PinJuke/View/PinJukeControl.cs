using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PinJuke.View
{
    public class PinJukeControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public PinJukeControl()
        {
            DataContext = this;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            // https://stackoverflow.com/a/1316417
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }
            field = value;
            NotifyPropertyChanged(propertyName);
        }
    }
}
