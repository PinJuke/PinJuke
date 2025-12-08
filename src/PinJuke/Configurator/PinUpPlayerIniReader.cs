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
    public record PinUpRect(int Left, int Top, int Width, int Height);

    public class PinUpPlayerIniReader
    {
        private const string BALLER_DIRECTORY_PATH = "vPinball";
        public const string BALLER_PIN_UP_PLAYER_INI = @"PinUPSystem\PinUpPlayer.ini";

        public static PinUpPlayerIniReader Create()
        {
            return new PinUpPlayerIniReader(new()
            {
                Directory.GetCurrentDirectory(),
                Path.Join(Path.GetPathRoot(Directory.GetCurrentDirectory()), BALLER_DIRECTORY_PATH),
                Path.Join("C:", BALLER_DIRECTORY_PATH),
                Path.Join("D:", BALLER_DIRECTORY_PATH),
            });
        }

        public IReadOnlyList<string> Paths { get; }
        protected IniReader IniReader { get; } = new();
        protected Parser Parser { get; } = new();

        public PinUpPlayerIniReader(List<string> paths)
        {
            Paths = new List<string>(paths).AsReadOnly();
        }

        public PinUpRect? FindPosition(DisplayRole displayRole)
        {
            var path = Search();
            if (path == null)
            {
                return null;
            }
            var iniDocument = IniReader.Read(path);

            string sectionName;
            switch (displayRole)
            {
                case DisplayRole.PlayField:
                    sectionName = "INFO3";
                    break;
                case DisplayRole.BackGlass:
                    sectionName = "INFO2";
                    break;
                case DisplayRole.DMD:
                    sectionName = Parser.ParseInt(iniDocument["INFO5"]["hidestopped"]) == 0 ? "INFO5" : "INFO1";
                    break;
                case DisplayRole.Topper:
                    sectionName = "INFO";
                    break;
                default:
                    return null;
            }
            var section = iniDocument[sectionName];
            var left = Parser.ParseInt(section["ScreenXPos"]);
            var top = Parser.ParseInt(section["ScreenYPos"]);
            var width = Parser.ParseInt(section["ScreenWidth"]);
            var height = Parser.ParseInt(section["ScreenHeight"]);
            if (left == null || top == null || width == null || height == null)
            {
                return null;
            }
            return new PinUpRect((int)left, (int)top, (int)width, (int)height);
        }

        public string? Search()
        {
            foreach (var eachPath in Paths)
            {
                var path = eachPath;
                while (!path.IsNullOrEmpty())
                {
                    var check = Path.Join(path, BALLER_PIN_UP_PLAYER_INI);
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
