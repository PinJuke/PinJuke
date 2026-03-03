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
    public class InputManager : IDisposable
    {
        public event EventHandler<InputActionEventArgs>? InputEvent;
        public event EventHandler<InputActionEventArgs>? ExitEvent;
        public event EventHandler<InputActionEventArgs>? BrowseEvent;
        public event EventHandler<InputActionEventArgs>? PreviousEvent;
        public event EventHandler<InputActionEventArgs>? NextEvent;
        public event EventHandler<InputActionEventArgs>? PlayPauseEvent;
        public event EventHandler<InputActionEventArgs>? VolumeDownEvent;
        public event EventHandler<InputActionEventArgs>? VolumeUpEvent;
        public event EventHandler<InputActionEventArgs>? TiltEvent;

        private readonly Configuration.Configuration configuration;

        public InputManager(Configuration.Configuration configuration)
        {
            this.configuration = configuration;
        }

        public void Dispose()
        {
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
            else if (key == keyboard.Tilt)
            {
                eventArgs = new(InputAction.Tilt, repeated);
                TiltEvent?.Invoke(this, eventArgs);
            }

            if (eventArgs != null)
            {
                InputEvent?.Invoke(this, eventArgs);
            }

            return eventArgs != null;
        }

        public void HandleGamepadButtonPressed(GamepadButtonEventArgs e)
        {
            var buttonNumber = e.ButtonNumber;
            var repeated = e.IsRepeated;

            InputActionEventArgs? eventArgs = null;
            var controller = configuration.Controller ?? throw new InvalidOperationException("Cannot handle gamepad button. Controller is null.");

            if (e.ButtonNumber == controller.Exit)
            {
                eventArgs = new(InputAction.Exit, repeated);
                ExitEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.Browse)
            {
                eventArgs = new(InputAction.Browse, repeated);
                BrowseEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.Previous)
            {
                eventArgs = new(InputAction.Previous, repeated);
                PreviousEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.Next)
            {
                eventArgs = new(InputAction.Next, repeated);
                NextEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.PlayPause)
            {
                eventArgs = new(InputAction.PlayPause, repeated);
                PlayPauseEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.VolumeDown)
            {
                eventArgs = new(InputAction.VolumeDown, repeated);
                VolumeDownEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.VolumeUp)
            {
                eventArgs = new(InputAction.VolumeUp, repeated);
                VolumeUpEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.Tilt)
            {
                eventArgs = new(InputAction.Tilt, repeated);
                TiltEvent?.Invoke(this, eventArgs);
            }

            if (eventArgs != null)
            {
                InputEvent?.Invoke(this, eventArgs);
            }
        }
    }
}
