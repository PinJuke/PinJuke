using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace PinJuke.View.Visualizer
{
    public partial class VisualizerControl : UserControl
    {
        public VisualizerManager? VisualizerManager { get; set; }

        private ProjectMRenderer projectMRenderer;
        private ProjectMPlaylist projectMPlaylist;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var visualizerManager = VisualizerManager;
            if (visualizerManager == null)
            {
                return;
            }

            projectMRenderer.SetTextureSearchPaths(new string[] { visualizerManager.Milkdrop.TexturesPath });
            projectMPlaylist.AddPath(visualizerManager.Milkdrop.PresetsPath, true, false);
            projectMPlaylist.SetShuffle(true);
            projectMPlaylist.PlayNext(true);

            visualizerManager.Add(projectMRenderer);
        }

        private void Dispatcher_ShutdownStarted(object? sender, EventArgs e)
        {
            Debug.WriteLine("Dispatcher_ShutdownStarted");

            VisualizerManager?.Remove(projectMRenderer);

            projectMPlaylist.Dispose();
            projectMRenderer.Dispose();
        }

        private void OpenTkControl_OnRender(TimeSpan delta)
        {
            //GL.ClearColor(Color4.Blue);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            projectMRenderer?.Render();
        }

        private void OpenTkControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            projectMRenderer?.SetSize((nuint)OpenTkControl.RenderSize.Width, (nuint)OpenTkControl.RenderSize.Height);
        }

    }
}
