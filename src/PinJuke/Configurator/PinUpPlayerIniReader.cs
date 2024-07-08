using PinJuke.Configuration;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Configurator
{
    public class PinUpPlayerIniReader
    {
        public IReadOnlyList<string> Paths { get; }
        protected IniReader IniReader { get; } = new();
        protected Parser Parser { get; } = new();

        public PinUpPlayerIniReader(List<string> paths)
        {
            Paths = new List<string>(paths).AsReadOnly();
        }

        public (int, int, int, int)? FindPosition(string sectionName)
        {
            var path = Search();
            if (path == null)
            {
                return null;
            }
            var iniDocument = IniReader.Read(path);
            var section = iniDocument[sectionName];
            var left = Parser.ParseInt(section["ScreenXPos"]);
            var top = Parser.ParseInt(section["ScreenYPos"]);
            var width = Parser.ParseInt(section["ScreenWidth"]);
            var height = Parser.ParseInt(section["ScreenHeight"]);
            if (left == null || top == null || width == null || height == null)
            {
                return null;
            }
            return ((int)left, (int)top, (int)width, (int)height);
        }

        public string? Search()
        {
            foreach (var eachPath in Paths)
            {
                var path = eachPath;
                while (!path.IsNullOrEmpty())
                {
                    var check = Path.Combine(path, "PinUPSystem", "PinUpPlayer.ini");
                    if (File.Exists(check))
                    {
                        return check;
                    }
                    path = Path.GetDirectoryName(path);
                }
            }
            return null;
        }
    }
}
