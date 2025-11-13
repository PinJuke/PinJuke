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
        private readonly GamepadManager? gamepadManager;
        private bool disposed = false;

        public InputManager(Configuration.Configuration configuration)
        {
            this.configuration = configuration;
            
            // Initialize gamepad manager if controller configuration exists
            if (configuration.Controller != null)
            {
                try
                {
                    gamepadManager = new GamepadManager();
                    gamepadManager.ButtonPressed += GamepadManager_ButtonPressed;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize gamepad manager: {ex.Message}");
                    gamepadManager = null;
                }
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            if (gamepadManager != null)
            {
                gamepadManager.ButtonPressed -= GamepadManager_ButtonPressed;
                gamepadManager.Dispose();
            }
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

        private void GamepadManager_ButtonPressed(object? sender, GamepadButtonEventArgs e)
        {
            if (configuration.Controller == null) return;

            var controller = configuration.Controller;
            InputActionEventArgs? eventArgs = null;

            if (e.ButtonNumber == controller.Exit)
            {
                eventArgs = new(InputAction.Exit, false);
                ExitEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.Browse)
            {
                eventArgs = new(InputAction.Browse, false);
                BrowseEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.Previous)
            {
                eventArgs = new(InputAction.Previous, false);
                PreviousEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.Next)
            {
                eventArgs = new(InputAction.Next, false);
                NextEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.PlayPause)
            {
                eventArgs = new(InputAction.PlayPause, false);
                PlayPauseEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.VolumeDown)
            {
                eventArgs = new(InputAction.VolumeDown, false);
                VolumeDownEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.VolumeUp)
            {
                eventArgs = new(InputAction.VolumeUp, false);
                VolumeUpEvent?.Invoke(this, eventArgs);
            }
            else if (e.ButtonNumber == controller.Tilt)
            {
                eventArgs = new(InputAction.Tilt, false);
                TiltEvent?.Invoke(this, eventArgs);
            }

            if (eventArgs != null)
            {
                InputEvent?.Invoke(this, eventArgs);
            }
        }

        public GamepadManager? GetGamepadManager()
        {
            return gamepadManager;
        }
    }
}
