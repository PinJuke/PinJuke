using PinJuke.Audio;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Controller
{
    public class DisplayController
    {
        private readonly MainModel mainModel;
        private readonly MainWindow window;
        private readonly AudioManager audioManager;

        private readonly InputManager inputManager;

        public DisplayController(MainModel mainModel, MainWindow window, AudioManager audioManager)
        {
            this.mainModel = mainModel;
            this.window = window;
            this.audioManager = audioManager;

            inputManager = new(mainModel.Configuration);

            window.Closed += Window_Closed;
            window.KeyDown += Window_KeyDown;
            window.MediaEndedEvent += Window_MediaEndedEvent;

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

        private void Window_Closed(object? sender, EventArgs e)
        {
            Debug.WriteLine("A window closed. Shutting down...");
            mainModel.Shutdown();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = inputManager.HandleKeyDown(e);
        }

        private void Window_MediaEndedEvent(object? sender, EventArgs e)
        {
            switch (mainModel.SceneType)
            {
                case SceneType.Intro:
                    mainModel.IntroEnded();
                    break;
                case SceneType.Playback:
                    mainModel.PlayNext();
                    break;
            }
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
                mainModel.PlayOrFollowDirectory();
            }
            else
            {
                mainModel.ShowBrowser();
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
                mainModel.PlayPrevious();
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
                mainModel.PlayNext();
            }
        }

        private void InputManager_PlayPauseEvent(object? sender, InputActionEventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                if (mainModel.NavigationNode == mainModel.PlayingFile)
                {
                    mainModel.TogglePlayPause();
                }
                else
                {
                    mainModel.PlayFile(mainModel.NavigationNode);
                    // Select the actual track.
                    mainModel.NavigateTo(mainModel.PlayingFile ?? mainModel.NavigationNode);
                }
            }
            else
            {
                if (mainModel.PlayingFile == null)
                {
                    mainModel.PlayFile(mainModel.NavigationNode);
                }
                else
                {
                    mainModel.TogglePlayPause();
                }
            }
        }

        private void InputManager_VolumeDownEvent(object? sender, InputActionEventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.ShowMilkdropInfo();
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
                mainModel.ShowMilkdropInfo();
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
        }
    }
}
