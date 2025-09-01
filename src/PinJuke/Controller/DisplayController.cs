using PinJuke.Audio;
using PinJuke.Model;
using PinJuke.View.Media;
using PinJuke.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace PinJuke.Controller
{
    public class DisplayController : IDisposable
    {
        private readonly MainModel mainModel;
        private readonly AudioManager audioManager;

        private readonly InputManager inputManager;

        private int tiltLastPlayed = 0;
        private readonly SoundPlayer tiltSoundPlayer;

        public DisplayController(MainModel mainModel, AudioManager audioManager)
        {
            this.mainModel = mainModel;
            this.audioManager = audioManager;

            inputManager = new(mainModel.Configuration);

            var uri = new Uri(@"resources\record-needle-rip-100415-edit.wav", UriKind.Relative);
            using var stream = Application.GetResourceStream(uri).Stream;
            tiltSoundPlayer = new SoundPlayer(stream);
            tiltSoundPlayer.Load();

            inputManager.InputEvent += InputManager_InputEvent;
            inputManager.ExitEvent += InputManager_ExitEvent;
            inputManager.BrowseEvent += InputManager_BrowseEvent;
            inputManager.PreviousEvent += InputManager_PreviousEvent;
            inputManager.NextEvent += InputManager_NextEvent;
            inputManager.PlayPauseEvent += InputManager_PlayPauseEvent;
            inputManager.VolumeDownEvent += InputManager_VolumeDownEvent;
            inputManager.VolumeUpEvent += InputManager_VolumeUpEvent;
            inputManager.TiltEvent += InputManager_TiltEvent;
        }

        public void Dispose()
        {
            tiltSoundPlayer.Dispose();
        }

        public void ObserveWindow(MainWindow window)
        {
            window.ShutdownRequestedEvent += Window_ShutdownRequested;
            window.KeyDown += Window_KeyDown;
        }

        private void Window_ShutdownRequested(object? sender, EventArgs e)
        {
            var window = (Window)sender!;
            Debug.WriteLine($@"{window.Title} requested shutdown.");
            mainModel.Shutdown();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = inputManager.HandleKeyDown(e);
        }

        private void InputManager_InputEvent(object? sender, InputActionEventArgs e)
        {
            mainModel.Input(e);
        }

        private void InputManager_ExitEvent(object? sender, InputActionEventArgs e)
        {
            mainModel.Shutdown();
        }

        private void InputManager_BrowseEvent(object? sender, InputActionEventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.PlayOrFollowDirectory(TriggerType.Manual);
            }
            else
            {
                mainModel.ShowBrowser();
                mainModel.ShowPresetInfo();
            }
        }

        private void InputManager_PreviousEvent(object? sender, InputActionEventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.NavigatePrevious(e.Repeated);
            }
            else
            {
                mainModel.PlayPrevious(TriggerType.Manual);
            }
        }

        private void InputManager_NextEvent(object? sender, InputActionEventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.NavigateNext(e.Repeated);
            }
            else
            {
                mainModel.PlayNext(TriggerType.Manual);
            }
        }

        private void InputManager_PlayPauseEvent(object? sender, InputActionEventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                if (mainModel.NavigationNode == mainModel.PlayingFile)
                {
                    mainModel.TogglePlayPause(TriggerType.Manual);
                }
                else
                {
                    mainModel.PlayFile(mainModel.NavigationNode, PlayFileType.Play, TriggerType.Manual);
                    // Select the actual track.
                    mainModel.NavigateTo(mainModel.PlayingFile ?? mainModel.NavigationNode);
                }
            }
            else
            {
                if (mainModel.PlayingFile == null)
                {
                    mainModel.PlayNext(TriggerType.Manual);
                }
                else
                {
                    mainModel.TogglePlayPause(TriggerType.Manual);
                }
            }
        }

        private void InputManager_VolumeDownEvent(object? sender, InputActionEventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.TriggerPreviousPreset();
            }
            else
            {
                var volumeLevel = audioManager.AddVolumeLevel(-0.05f);
                ShowVolumeLevel(volumeLevel);
            }
        }

        private void InputManager_VolumeUpEvent(object? sender, InputActionEventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.TriggerNextPreset();
            }
            else
            {
                var volumeLevel = audioManager.AddVolumeLevel(0.05f);
                ShowVolumeLevel(volumeLevel);
            }
        }

        private void ShowVolumeLevel(float? volumeLevel)
        {
            mainModel.ShowState(new State(StateType.Volume, volumeLevel));
        }

        private void InputManager_TiltEvent(object? sender, InputActionEventArgs e)
        {
            mainModel.ShowState(new State(StateType.Tilt));

            var now = Environment.TickCount;
            if (now - tiltLastPlayed >= 2_000)
            {
                tiltLastPlayed = now;
                tiltSoundPlayer.Play();
            }
        }
    }
}
