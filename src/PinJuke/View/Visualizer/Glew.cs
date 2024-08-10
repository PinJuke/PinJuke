using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.View.Visualizer
{
    public class GlewException : Exception
    {
        public uint Error { get; }

        public GlewException(string message, uint error) : base(message)
        {
            Error = error;
        }
    }

    public class Glew
    {
        public static void Initialize()
        {
            var error = LibGlew.Init();
            if (error != LibGlew.GLEW_OK)
            {
                throw new GlewException(string.Format("Glew initialization failed ({0}).", error), error);
            }
        }
    }
}
