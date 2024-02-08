using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Model
{
    public enum InputAction
    {
        Exit,
        Browse,
        Previous,
        Next,
        PlayPause,
        VolumeDown,
        VolumeUp,
        Tilt,
    }

    public class InputActionEventArgs : EventArgs
    {
        public InputAction InputAction { get; }
        public bool Repeated { get; }

        public InputActionEventArgs(InputAction inputAction, bool repeated)
        {
            InputAction = inputAction;
            Repeated = repeated;
        }
    }
}
