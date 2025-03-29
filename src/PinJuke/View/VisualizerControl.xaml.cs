using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using PinJuke.Audio;
using PinJuke.Configuration;
using PinJuke.View;
using PinJuke.View.Visualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PinJuke.View
{
    public partial class VisualizerControl : UserControl
    {
        private Visualizer.Visualizer? visualizer = null;
        private Milkdrop? milkdrop = null;

        public bool PresetInfoVisible
        {
            get => PresetInfoControl.ViewVisible;
            set
            {
                SetPresetInfoVisible(value);
            }
        }

        public VisualizerControl()
        {
            InitializeComponent();
            Loaded += VisualizerControl_Loaded;
            Unloaded += VisualizerControl_Unloaded;

            var settings = new OpenTK.Wpf.GLWpfControlSettings();
            OpenTkControl.Start(settings);

            Glew.Initialize();
        }

        private void VisualizerControl_Loaded(object sender, RoutedEventArgs e)
        {
            OpenTkControl.Context!.MakeCurrent();
            visualizer = new();
            UpdateSize();
            // https://stackoverflow.com/questions/1550212/proper-cleanup-of-wpf-user-controls
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            OpenTkControl.Render += OpenTkControl_Render;
            OpenTkControl.SizeChanged += OpenTkControl_SizeChanged;
        }

        private void VisualizerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unload();
        }

        private void Dispatcher_ShutdownStarted(object? sender, EventArgs e)
        {
            Unload();
        }

        private void Unload()
        {
            OpenTkControl.Context!.MakeCurrent();
            Dispatcher.ShutdownStarted -= Dispatcher_ShutdownStarted;
            OpenTkControl.Render -= OpenTkControl_Render;
            OpenTkControl.SizeChanged -= OpenTkControl_SizeChanged;
            visualizer?.Dispose();
            visualizer = null;
        }

        public void Initialize(AudioManager audioManager, Milkdrop milkdrop)
        {
            visualizer!.Initialize(audioManager, milkdrop);
            this.milkdrop = milkdrop;
        }

        private void OpenTkControl_Render(TimeSpan delta)
        {
            //GL.ClearColor(Color4.Blue);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var framebuffer = OpenTkControl.Framebuffer;
            visualizer!.Render(framebuffer);
        }

        private void OpenTkControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            visualizer!.SetSize((int)OpenTkControl.RenderSize.Width, (int)OpenTkControl.RenderSize.Height);
        }

        private void SetPresetInfoVisible(bool visible)
        {
            if (visible)
            {
                UpdatePresetInfoText();
            }
            PresetInfoControl.ViewVisible = visible;
        }

        private void UpdatePresetInfoText()
        {
            var item = visualizer?.GetCurrentItem();
            var presetsPath = milkdrop?.PresetsPath;
            if (item != null && presetsPath != null)
            {
                if (!presetsPath.EndsWith('\\'))
                {
                    presetsPath += '\\';
                }
                if (item.ToLowerInvariant().StartsWith(presetsPath.ToLowerInvariant()))
                {
                    item = item.Substring(presetsPath.Length);
                }
            }
            PresetInfoControl.StateText = item;
        }

        public void PlayNext()
        {
            OpenTkControl.Context!.MakeCurrent();
            visualizer?.PlayNext(false);
            UpdatePresetInfoText();
        }

        public void PlayPrevious()
        {
            OpenTkControl.Context!.MakeCurrent();
            visualizer?.PlayLast(false);
            UpdatePresetInfoText();
        }
    }
}
