using PinJuke.Audio;
using PinJuke.Configuration;
using PinJuke.Controller;
using PinJuke.Dof;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke
{
    public partial class App : Application
    {
        private AppController? appController;
        private DofMediator? dofMediator = null;
        private AudioManager? audioManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            Configuration.Configuration configuration;
            try
            {
                configuration = LoadConfiguration(e);
            }
            catch (IniIoException ex)
            {
                MessageBox.Show(ex.Message, AppDomain.CurrentDomain.FriendlyName);
                Application.Current.Shutdown(1);
                return;
            }

            Unosquare.FFME.Library.FFmpegDirectory = @"ffmpeg";

            var mainModel = new MainModel(configuration);

            if (configuration.Dof.Enabled)
            {
                dofMediator = new DofMediator(mainModel, configuration.Dof);
            }

            audioManager = new();
            var playFieldWindow = CreateWindow(mainModel, configuration.PlayField, audioManager);
            var backGlassWindow = CreateWindow(mainModel, configuration.BackGlass, audioManager);
            var dmdWindow = CreateWindow(mainModel, configuration.Dmd, audioManager);

            appController = new AppController(mainModel);
            appController.Scan();

            playFieldWindow?.Show();
            backGlassWindow?.Show();
            dmdWindow?.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            appController?.Dispose();
            audioManager?.Dispose();
            dofMediator?.Dispose();
        }

        private Configuration.Configuration LoadConfiguration(StartupEventArgs e)
        {
            List<string> iniFilePaths = new();
            iniFilePaths.Add(@"Configs\PinJuke.global.ini");
            if (e.Args.Length >= 1)
            {
                iniFilePaths.Add(e.Args[0]);
            }

            var loader = new Configuration.Loader();
            return loader.FromIniFilePaths(iniFilePaths);
        }

        private MainWindow? CreateWindow(MainModel mainModel, Configuration.Display displayConfig, AudioManager audioManager)
        {
            if (!displayConfig.Enabled)
            {
                return null;
            }
            var window = new MainWindow(mainModel, displayConfig, audioManager);
            var controller = new DisplayController(mainModel, window, audioManager);
            return window;
        }
    }
}
