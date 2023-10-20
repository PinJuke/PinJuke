using PinJuke.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            List<string> iniFilePaths = new();
            iniFilePaths.Add("PinJuke.global.ini");
            if (e.Args.Length >= 1)
            {
                iniFilePaths.Add(e.Args[0]);
            }

            var loader = new Loader();
            var configuration = loader.FromIniFilePaths(iniFilePaths);
        }
    }
}
