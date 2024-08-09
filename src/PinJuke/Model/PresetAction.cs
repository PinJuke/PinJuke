using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Model
{
    public enum PresetAction
    {
        Next,
        Previous,
    }

    public class PresetActionEventArgs : EventArgs
    {
        public PresetAction PresetAction { get; }

        public PresetActionEventArgs(PresetAction presetAction)
        {
            PresetAction = presetAction;
        }
    }
}
