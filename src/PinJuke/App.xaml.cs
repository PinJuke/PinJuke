using PinJuke.Configuration;
using PinJuke.Controller;
using PinJuke.Model;
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
        private AppController? appController;

        protected override void OnStartup(StartupEventArgs e)
        {
            List<string> iniFilePaths = new();
            iniFilePaths.Add("PinJuke.global.ini");
            if (e.Args.Length >= 1)
            {
                iniFilePaths.Add(e.Args[0]);
            }

            var loader = new Configuration.Loader();
            var configuration = loader.FromIniFilePaths(iniFilePaths);

            var mainModel = new MainModel(configuration);

            var backGlassWindow = CreateWindow(mainModel, configuration.BackGlass);
            var playFieldWindow = CreateWindow(mainModel, configuration.PlayField);
            var dmdWindow = CreateWindow(mainModel, configuration.DMD);

            appController = new AppController(mainModel);
            appController.Scan();

            backGlassWindow?.Show();
            playFieldWindow?.Show();
            dmdWindow?.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            appController?.Dispose();
        }

        private MainWindow? CreateWindow(MainModel mainModel, Configuration.Display displayConfig)
        {
            if (!displayConfig.Enabled)
            {
                return null;
            }
            var window = new MainWindow(mainModel, displayConfig);
            return window;
        }
    }
}
