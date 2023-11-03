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
        public static extern nuint Create();

        [DllImport("projectM-4.dll", EntryPoint = "projectm_destroy")]
        public static extern void Destroy(nuint projectMHandle);

        [DllImport("projectM-4.dll", EntryPoint = "projectm_set_window_size")]
        public static extern void SetWindowSize(nuint projectMHandle, nuint width, nuint size);

        [DllImport("projectM-4.dll", EntryPoint = "projectm_opengl_render_frame")]
        public static extern void OpenglRenderFrame(nuint projectMHandle);

        [DllImport("projectM-4.dll", EntryPoint = "projectm_set_texture_search_paths")]
        public static extern void SetTextureSearchPaths(nuint projectMHandle, string[] textureSearchPaths, nuint count);

        [DllImport("projectM-4.dll", EntryPoint = "projectm_pcm_add_float")]
        public static extern void PcmAddFloat(nuint projectMHandle, byte[] samples, uint count, uint channels);

    }
}
