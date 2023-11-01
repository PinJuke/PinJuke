using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View.Visualizer
{
    public class LibProjectM
    {
        [DllImport("projectM-4.dll", EntryPoint = "projectm_create")]
        public static extern nint Create();

        [DllImport("projectM-4.dll", EntryPoint = "projectm_destroy")]
        public static extern void Destroy(nint projectMHandle);

        [DllImport("projectM-4.dll", EntryPoint = "projectm_set_window_size")]
        public static extern void SetWindowSize(nint projectMHandle, nint width, nint size);

        [DllImport("projectM-4.dll", EntryPoint = "projectm_opengl_render_frame")]
        public static extern void OpenglRenderFrame(nint projectMHandle);

        [DllImport("projectM-4.dll", EntryPoint = "projectm_set_texture_search_paths")]
        public static extern void SetTextureSearchPaths(nint projectMHandle, string[] textureSearchPaths, nint count);

    }
}
