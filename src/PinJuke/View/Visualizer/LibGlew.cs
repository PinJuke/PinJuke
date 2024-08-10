using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View.Visualizer
{
    public class LibGlew
    {
        public const uint GLEW_OK = 0;

        private const string DLL = "glew32.dll";

        [DllImport(DLL, EntryPoint = "glewInit")]
        public static extern uint Init();
    }
}
