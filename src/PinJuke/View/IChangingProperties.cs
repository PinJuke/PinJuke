using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View
{
    public interface IChangingProperties : INotifyPropertyChanged
    {
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "");
    }
}
