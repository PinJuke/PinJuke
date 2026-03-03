using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using SharpDX.DirectInput;

namespace PinJuke.Controller
{
    public class GamepadManager : IDisposable
    {
        private readonly DirectInput directInput;
        private readonly List<Joystick> connectedGamepads;
        private readonly CancellationTokenSource cancellationTokenSource;
        private Task? pollingTask = null;
        private bool disposed = false;

        // Events for button presses
        public event EventHandler<GamepadButtonEventArgs>? ButtonPressed;

        public GamepadManager()
        {
            directInput = new DirectInput();
            connectedGamepads = new List<Joystick>();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            cancellationTokenSource?.Cancel();
            
            try
            {
                pollingTask?.Wait(1000); // Wait up to 1 second for polling to stop
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }

            foreach (var gamepad in connectedGamepads)
            {
                try
                {
                    gamepad?.Unacquire();
                    gamepad?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error disposing gamepad: {ex.Message}");
                }
            }

            connectedGamepads.Clear();
            directInput?.Dispose();
            cancellationTokenSource?.Dispose();
        }

        public void Start()
        {
            if (disposed)
            {
                throw new InvalidOperationException("Cannot start GamepadManager. Already disposed.");
            }

            // Initialize gamepads
            InitializeGamepads();

            // Start polling task
            pollingTask = Task.Run(PollGamepads, cancellationTokenSource.Token);
        }

        private void InitializeGamepads()
        {
            try
            {
                // Get all joystick/gamepad devices
                var devices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

                foreach (var deviceInstance in devices)
                {
                    try
                    {
                        var joystick = new Joystick(directInput, deviceInstance.InstanceGuid);

                        // Acquire the joystick
                        joystick.Acquire();

                        connectedGamepads.Add(joystick);
                        Debug.WriteLine($"Gamepad connected: {deviceInstance.ProductName} ({connectedGamepads.Count} total)");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to initialize gamepad {deviceInstance.ProductName}: {ex.Message}");
                    }
                }

                if (connectedGamepads.Count == 0)
                {
                    Debug.WriteLine("No gamepads detected");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing gamepads: {ex.Message}");
            }
        }

        private async Task PollGamepads()
        {
            var triggerNextAtByGamepad = new Dictionary<Joystick, long[]>();

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    foreach (var gamepad in connectedGamepads.ToList())
                    {
                        try
                        {
                            // Poll the gamepad
                            gamepad.Poll();
                            var state = gamepad.GetCurrentState();

                            // Initialize previous state if not exists
                            if (!triggerNextAtByGamepad.ContainsKey(gamepad))
                            {
                                triggerNextAtByGamepad[gamepad] = new long[128]; // DirectInput supports up to 128 buttons
                            }

                            var triggerNextAt = triggerNextAtByGamepad[gamepad];
                            var currentButtonsDown = state.Buttons;

                            var len = Math.Min(currentButtonsDown.Length, triggerNextAt.Length);
                            var now = Environment.TickCount64;

                            // Check for button press events (transition from false to true)
                            for (int i = 0; i < len; i++)
                            {
                                if (currentButtonsDown[i])
                                {
                                    var isRepeated = triggerNextAt[i] != 0;
                                    if (!isRepeated || now >= triggerNextAt[i])
                                    {
                                        triggerNextAt[i] = now + (isRepeated ? 100 : 250);

                                        // Button is pressed - marshal to UI thread
                                        var buttonNumber = i + 1; // 1-based button numbering
                                        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            ButtonPressed?.Invoke(this, new GamepadButtonEventArgs(buttonNumber, isRepeated));
                                        });
                                    }
                                }
                                else
                                {
                                    triggerNextAt[i] = 0;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error polling gamepad: {ex.Message}");
                            // Remove problematic gamepad
                            connectedGamepads.Remove(gamepad);
                            if (triggerNextAtByGamepad.ContainsKey(gamepad))
                            {
                                triggerNextAtByGamepad.Remove(gamepad);
                            }
                        }
                    }

                    // Sleep for a short interval to avoid excessive CPU usage
                    await Task.Delay(16, cancellationTokenSource.Token); // ~60 FPS polling
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in gamepad polling loop: {ex.Message}");
                    await Task.Delay(100, cancellationTokenSource.Token); // Wait longer on error
                }
            }
        }

        public List<string> GetConnectedGamepadNames()
        {
            return connectedGamepads.Select((gamepad, index) => 
                $"Controller {index + 1}").ToList();
        }

        public int GetConnectedGamepadCount()
        {
            return connectedGamepads.Count;
        }
    }

    public class GamepadButtonEventArgs : EventArgs
    {
        public int ButtonNumber { get; }
        public bool IsRepeated { get; }

        public GamepadButtonEventArgs(int buttonNumber, bool isRepeated)
        {
            ButtonNumber = buttonNumber;
            IsRepeated = isRepeated;
        }
    }
}
