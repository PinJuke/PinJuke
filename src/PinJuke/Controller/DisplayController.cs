﻿using PinJuke.Model;
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

        private readonly InputManager inputManager;

        public DisplayController(MainModel mainModel, MainWindow window)
        {
            this.mainModel = mainModel;
            this.window = window;

            inputManager = new(mainModel.Configuration);

            window.Closed += Window_Closed;
            window.KeyDown += Window_KeyDown;
            inputManager.InputEvent += InputManager_InputEvent;
            inputManager.ExitEvent += InputManager_ExitEvent;
            inputManager.BrowseEvent += InputManager_BrowseEvent;
            inputManager.PreviousEvent += InputManager_PreviousEvent;
            inputManager.NextEvent += InputManager_NextEvent;
            inputManager.PlayPauseEvent += InputManager_PlayPauseEvent;
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            Debug.WriteLine("A window closed. Shutting down...");
            mainModel.Shutdown();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = inputManager.HandleKeyDown(e.Key);
        }

        private void InputManager_InputEvent(object? sender, InputActionEventArgs e)
        {
            mainModel.Input(e);
        }

        private void InputManager_ExitEvent(object? sender, EventArgs e)
        {
            mainModel.Shutdown();
        }

        private void InputManager_BrowseEvent(object? sender, EventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.DescendOrPlay();
            }
            else
            {
                mainModel.ShowBrowser();
            }
        }

        private void InputManager_PreviousEvent(object? sender, EventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.NavigatePrevious();
            }
            else
            {
                mainModel.PlayPrevious();
            }
        }

        private void InputManager_NextEvent(object? sender, EventArgs e)
        {
            if (mainModel.BrowserVisible)
            {
                mainModel.NavigateNext();
            }
            else
            {
                mainModel.PlayNext();
            }
        }

        private void InputManager_PlayPauseEvent(object? sender, EventArgs e)
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
                }
            }
            else
            {
                mainModel.TogglePlayPause();
            }
        }

    }
}
