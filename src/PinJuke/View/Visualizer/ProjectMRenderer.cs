using OpenTK.Windowing.GraphicsLibraryFramework;
using PinJuke.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View.Visualizer
{
    public class ProjectMRendererException : Exception
    {
        public ProjectMRendererException(string message) : base(message)
        {
        }
    }

    public class ProjectMRenderer : IDisposable, PcmDataListener
    {
        private nuint handle = nuint.Zero;
        internal nuint Handle
        {
            get => handle != nuint.Zero ? handle : throw new ProjectMRendererException("ProjectM handle is null.");
        }

        internal ProjectMPlaylist? Playlist { get; set; } = null;

        public ProjectMRenderer()
        {
            handle = LibProjectM.Create();
            if (handle == nuint.Zero)
            {
                throw new ProjectMRendererException("Failed to create a projectM instance.");
            }
        }

        ~ProjectMRenderer() => Release();

        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }

        internal void Release()
        {
            if (handle != nuint.Zero)
            {
                Playlist?.Release();

                LibProjectM.Destroy(handle);
                handle = nuint.Zero;
            }
        }

        public void SetTextureSearchPaths(string[] textureSearchPaths)
        {
            LibProjectM.SetTextureSearchPaths(Handle, textureSearchPaths, (nuint)textureSearchPaths.Length);
        }

        public void SetPresetDuration(double seconds)
        {
            LibProjectM.SetPresetDuration(Handle, seconds);
        }

        public void SetSize(nuint width, nuint height)
        {
            LibProjectM.SetWindowSize(Handle, width, height);
        }

        public void Render()
        {
            LibProjectM.OpenglRenderFrame(Handle);
        }

        public void OnPcmData(byte[] samples, uint count, uint channels)
        {
            LibProjectM.PcmAddFloat(Handle, samples, count, channels);
        }
    }
}
