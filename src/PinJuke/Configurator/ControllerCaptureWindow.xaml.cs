using PinJuke.Controller;
using PinJuke.Model;
using PinJuke.Playlist;
using PinJuke.Utility;
using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PinJuke.Configurator
{
    public partial class ControllerCaptureWindow : Window, IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<GamepadButtonEventArgs>? ButtonPressed;

        private GamepadManager? gamepadManager = null;

        public ControllerCaptureWindow()
        {
            DataContext = this;
            InitializeComponent();
            Loaded += Window_Loaded;
            Closed += Window_Closed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                gamepadManager = new GamepadManager();
                gamepadManager.Start();
                gamepadManager.ButtonPressed += GamepadManager_ButtonPressed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize gamepad manager: {ex.Message}");
                UiUtil.ShowErrorMessage(string.Format(Strings.ErrorGamepadInit, ex.ToString()));
                DisposeGamepadManager();
                Close();
                return;
            }

            if (gamepadManager.GetConnectedGamepadCount() == 0)
            {
                UiUtil.ShowErrorMessage(string.Format(Strings.ErrorNoGamepadFound));
                DisposeGamepadManager();
                Close();
                return;
            }
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            DisposeGamepadManager();
        }

        private void DisposeGamepadManager()
        {
            if (gamepadManager != null)
            {
                gamepadManager.ButtonPressed -= GamepadManager_ButtonPressed;
                gamepadManager.Dispose();
                gamepadManager = null;
            }
        }

        private void GamepadManager_ButtonPressed(object? sender, GamepadButtonEventArgs e)
        {
            ButtonPressed?.Invoke(this, e);
            Close();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
