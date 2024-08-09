using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PinJuke.Audio;
using PinJuke.Configuration;
using PinJuke.View.Visualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinJuke.View
{
    public partial class VisualizerControl : UserControl
    {
        private AudioManager? audioManager = null;
        private Milkdrop? milkdrop = null;

        private ProjectMRenderer projectMRenderer;
        private ProjectMPlaylist projectMPlaylist;

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
            // https://stackoverflow.com/questions/1550212/proper-cleanup-of-wpf-user-controls
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            InitializeComponent();

            // https://github.com/opentk/GLWpfControl/issues/82#issuecomment-1516503816
            OpenTkControl.RegisterToEventsDirectly = false;

            var settings = new OpenTK.Wpf.GLWpfControlSettings();
            OpenTkControl.Start(settings);

            projectMRenderer = new();
            projectMPlaylist = new();
            projectMPlaylist.Connect(projectMRenderer);
        }

        public void Initialize(AudioManager audioManager, Milkdrop milkdrop)
        {
            if (this.audioManager != null)
            {
                throw new InvalidOperationException("AudioManager is already set.");
            }

            this.audioManager = audioManager;
            this.milkdrop = milkdrop;

            projectMRenderer.SetTextureSearchPaths([milkdrop.TexturesPath]);
            projectMRenderer.SetPresetDuration(60);
            projectMPlaylist.AddPath(milkdrop.PresetsPath, true, false);
            projectMPlaylist.SetShuffle(true);
            projectMPlaylist.PlayNext(true);

            audioManager.AddPcmDataListener(projectMRenderer);
        }

        private void Dispatcher_ShutdownStarted(object? sender, EventArgs e)
        {
            Debug.WriteLine("Dispatcher_ShutdownStarted");

            audioManager?.RemovePcmDataListener(projectMRenderer);

            projectMPlaylist.Dispose();
            projectMRenderer.Dispose();
        }

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            //GL.ClearColor(Color4.Blue);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            projectMRenderer.Render();
        }

        private void OpenTkControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            projectMRenderer.SetSize((nuint)OpenTkControl.RenderSize.Width, (nuint)OpenTkControl.RenderSize.Height);
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
            var item = projectMPlaylist.GetCurrentItem();
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
            projectMPlaylist.PlayNext(false);
            UpdatePresetInfoText();
        }

        public void PlayPrevious()
        {
            projectMPlaylist.PlayLast(false);
            UpdatePresetInfoText();
        }
    }
}
