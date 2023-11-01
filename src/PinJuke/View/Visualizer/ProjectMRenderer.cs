using OpenTK.Windowing.GraphicsLibraryFramework;
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

    public class ProjectMRenderer : IDisposable
    {
        private nint handle = nint.Zero;
        internal nint Handle
        {
            get => handle != nint.Zero ? handle : throw new ProjectMRendererException("ProjectM handle is null.");
        }

        internal ProjectMPlaylist? Playlist { get; set; } = null;

        public ProjectMRenderer()
        {
            handle = LibProjectM.Create();
            if (handle == nint.Zero)
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
            if (handle != nint.Zero)
            {
                Playlist?.Release();

                LibProjectM.Destroy(handle);
                handle = nint.Zero;
            }
        }

        public void SetTextureSearchPaths(string[] textureSearchPaths)
        {
            LibProjectM.SetTextureSearchPaths(Handle, textureSearchPaths, textureSearchPaths.Length);
        }

        public void SetSize(nint width, nint height)
        {
            LibProjectM.SetWindowSize(Handle, width, height);
        }

        public void Render()
        {
            LibProjectM.OpenglRenderFrame(Handle);
        }

    }
}
