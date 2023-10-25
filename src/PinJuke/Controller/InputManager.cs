using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinJuke.Controller
{
    public class InputManager
    {
        public event EventHandler<InputActionEventArgs>? InputEvent;
        public event EventHandler<InputActionEventArgs>? ExitEvent;
        public event EventHandler<InputActionEventArgs>? BrowseEvent;
        public event EventHandler<InputActionEventArgs>? PreviousEvent;
        public event EventHandler<InputActionEventArgs>? NextEvent;
        public event EventHandler<InputActionEventArgs>? PlayPauseEvent;

        private readonly Configuration.Configuration configuration;

        public InputManager(Configuration.Configuration configuration)
        {
            this.configuration = configuration;
        }

        public bool HandleKeyDown(Key key)
        {
            InputActionEventArgs? eventArgs = null;
            var keyboard = configuration.Keyboard;
            if (key == keyboard.Exit)
            {
                eventArgs = new(InputAction.Exit);
                ExitEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.Browse)
            {
                eventArgs = new(InputAction.Browse);
                BrowseEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.Previous)
            {
                eventArgs = new(InputAction.Previous);
                PreviousEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.Next)
            {
                eventArgs = new(InputAction.Next);
                NextEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.PlayPause)
            {
                eventArgs = new(InputAction.PlayPause);
                PlayPauseEvent?.Invoke(this, eventArgs);
            }

            if (eventArgs != null)
            {
                InputEvent?.Invoke(this, eventArgs);
            }

            return eventArgs != null;
        }
    }
}
