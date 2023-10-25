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
    }

    public class InputActionEventArgs : EventArgs
    {
        public InputAction InputAction { get; }

        public InputActionEventArgs(InputAction inputAction)
        {
            InputAction = inputAction;
        }
    }
}
