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
    public class BaseControl : UserControl, IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public BaseControl()
        {
            DataContext = this;
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }
    }
}
