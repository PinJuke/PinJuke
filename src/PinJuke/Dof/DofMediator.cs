#if !DISABLE_DIRECTOUTPUT
using DirectOutput;
using DirectOutput.PinballSupport;
using DirectOutput.Table;
#endif
using PinJuke.Configuration;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PinJuke.Dof
{
    enum Lamp
    {
        Startup = 0,
        InputBrowse = 16,
        InputPrevious = 17,
        InputNext = 18,
        InputPlayPause = 19,
        InputVolumeDown = 20,
        InputVolumeUp = 21,
        InputTilt = 22,
        Volume1 = 32,
        Volume2 = 33,
        Volume3 = 34,
        Volume4 = 35,
        Volume5 = 36,
    }

    public class DofMediator : IDisposable
    {
        private static readonly Dictionary<InputAction, Lamp> inputLamps = new()
        {
            {InputAction.Browse, Lamp.InputBrowse},
            {InputAction.Previous, Lamp.InputPrevious},
            {InputAction.Next, Lamp.InputNext},
            {InputAction.PlayPause, Lamp.InputPlayPause},
            {InputAction.VolumeDown, Lamp.InputVolumeDown},
            {InputAction.VolumeUp, Lamp.InputVolumeUp},
            {InputAction.Tilt, Lamp.InputTilt},
        };

        private readonly MainModel mainModel;
        private readonly Configuration.Dof dof;
#if !DISABLE_DIRECTOUTPUT
        private readonly Pinball pinball;
#endif
        private bool disposed = false;
        private bool initialized = false;

        public bool Initialized { get { return initialized; } }

        public DofMediator(MainModel mainModel, Configuration.Dof dof)
        {
            this.mainModel = mainModel;
            this.dof = dof;

#if !DISABLE_DIRECTOUTPUT
            pinball = new Pinball();
#endif
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            mainModel.InputEvent -= MainModel_InputEvent;
            mainModel.PropertyChanged -= MainModel_PropertyChanged;
#if !DISABLE_DIRECTOUTPUT
            pinball.Finish();
#endif
        }

        public void Startup()
        {
#if !DISABLE_DIRECTOUTPUT
            try
            {
                pinball.Setup(dof.GlobalConfigFilePath, "", dof.RomName);
            }
            catch (Exception)
            {
                Debug.WriteLine("Error setting up DOF.");
                return;
            }
            setup = true;

            try
            {
                pinball.Init();
            }
            catch (Exception)
            {
                Debug.WriteLine("Error initializing DOF.");
                return;
            }
            initialized = true;

            mainModel.InputEvent += MainModel_InputEvent;
            mainModel.PropertyChanged += MainModel_PropertyChanged;

            Trigger(Lamp.Startup);
#else
            // DirectOutput disabled for testing
            initialized = true;
            mainModel.InputEvent += MainModel_InputEvent;
            mainModel.PropertyChanged += MainModel_PropertyChanged;
#endif
        }

        private void MainModel_InputEvent(object? sender, InputActionEventArgs e)
        {
            if (inputLamps.TryGetValue(e.InputAction, out var lamp))
            {
                Trigger(lamp);
            }
        }

        private void MainModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainModel.LastState):
                    LastStateChanged();
                    break;
            }
        }

        private void LastStateChanged()
        {
            var state = mainModel.LastState;

            if (state.Type == StateType.Volume)
            {
                var volumeLevel = (float?)state.Data;
                if (volumeLevel != null)
                {
                    var numFlasherLamps = 5;
                    var numActiveLamps = 1 + Math.Max(0, Math.Min(numFlasherLamps - 1, (int)(volumeLevel * numFlasherLamps)));
                    Debug.WriteLine("Num active volume lamps: " + numActiveLamps);
                    for (var i = 0; i < numActiveLamps; ++i)
                    {
                        Trigger(Lamp.Volume1 + i);
                    }
                }
            }
        }

        private void Trigger(Lamp lamp)
        {
#if !DISABLE_DIRECTOUTPUT
            pinball.ReceiveData((char)TableElementTypeEnum.Lamp, (int)lamp, 1); // Lamp on
            pinball.ReceiveData((char)TableElementTypeEnum.Lamp, (int)lamp, 0); // Lamp off
#endif
        }

        public string[] GetControllerNames()
        {
#if !DISABLE_DIRECTOUTPUT
            return pinball.Cabinet.OutputControllers
                .Select(controller => Regex.Replace(controller.Name, @"\s\d+$", ""))
                .Distinct()
                .OrderBy(str => str, StringComparer.InvariantCulture)
                .ToArray();
#else
            return new string[0];
#endif
        }
    }
}
