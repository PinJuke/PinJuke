using PinJuke.Configuration;
using PinJuke.Controller;
using PinJuke.Model;
using PinJuke.View.Visualizer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke
{
    public partial class App : Application
    {
        private AppController? appController;
        private VisualizerManager? visualizerManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            var configuration = LoadConfiguration(e);

            Unosquare.FFME.Library.FFmpegDirectory = @"ffmpeg";

            var mainModel = new MainModel(configuration);

            visualizerManager = new VisualizerManager(configuration.Milkdrop);
            var backGlassWindow = CreateWindow(mainModel, configuration.BackGlass, visualizerManager);
            var playFieldWindow = CreateWindow(mainModel, configuration.PlayField, visualizerManager);
            var dmdWindow = CreateWindow(mainModel, configuration.DMD, visualizerManager);

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
            visualizerManager?.Dispose();
        }

        private Configuration.Configuration LoadConfiguration(StartupEventArgs e)
        {
            List<string> iniFilePaths = new();
            iniFilePaths.Add("PinJuke.global.ini");
            if (e.Args.Length >= 1)
            {
                iniFilePaths.Add(e.Args[0]);
            }

            var loader = new Configuration.Loader();
            return loader.FromIniFilePaths(iniFilePaths);
        }

        private MainWindow? CreateWindow(MainModel mainModel, Configuration.Display displayConfig, VisualizerManager visualizerManager)
        {
            if (!displayConfig.Enabled)
            {
                return null;
            }
            var window = new MainWindow(mainModel, displayConfig, visualizerManager);
            var controller = new DisplayController(mainModel, window);
            return window;
        }
    }
}
