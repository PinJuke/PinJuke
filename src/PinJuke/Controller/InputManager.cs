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
        public event EventHandler<InputActionEventArgs>? VolumeDownEvent;
        public event EventHandler<InputActionEventArgs>? VolumeUpEvent;

        private readonly Configuration.Configuration configuration;

        public InputManager(Configuration.Configuration configuration)
        {
            this.configuration = configuration;
        }

        public bool HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key;
            var repeated = e.IsRepeat;

            InputActionEventArgs? eventArgs = null;
            var keyboard = configuration.Keyboard;
            if (key == keyboard.Exit)
            {
                eventArgs = new(InputAction.Exit, repeated);
                ExitEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.Browse)
            {
                eventArgs = new(InputAction.Browse, repeated);
                BrowseEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.Previous)
            {
                eventArgs = new(InputAction.Previous, repeated);
                PreviousEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.Next)
            {
                eventArgs = new(InputAction.Next, repeated);
                NextEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.PlayPause)
            {
                eventArgs = new(InputAction.PlayPause, repeated);
                PlayPauseEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.VolumeDown)
            {
                eventArgs = new(InputAction.VolumeDown, repeated);
                VolumeDownEvent?.Invoke(this, eventArgs);
            }
            else if (key == keyboard.VolumeUp)
            {
                eventArgs = new(InputAction.VolumeUp, repeated);
                VolumeUpEvent?.Invoke(this, eventArgs);
            }

            if (eventArgs != null)
            {
                InputEvent?.Invoke(this, eventArgs);
            }

            return eventArgs != null;
        }
    }
}
