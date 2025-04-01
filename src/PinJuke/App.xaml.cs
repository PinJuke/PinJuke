using PinJuke.Audio;
using PinJuke.Configurator;
using PinJuke.Controller;
using PinJuke.Dof;
using PinJuke.Ini;
using PinJuke.Model;
using PinJuke.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke
{
    public partial class App : Application
    {
        private MainModel? mainModel;
        private ConfigurationService configurationService = new();
        private BeaconService beaconService = new();
        private BeaconController? beaconController = null;
        private AppController? appController = null;
        private DisplayController? displayController = null;
        private DofMediator? dofMediator = null;
        private AudioManager? audioManager = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool parsingOptions = true;
            string? playlistConfigFilePath = null;
            bool configurator = false;

            foreach (var arg in e.Args)
            {
                if (parsingOptions && arg.StartsWith('-'))
                {
                    switch (arg)
                    {
                        case "--":
                            parsingOptions = false;
                            break;
                        case "--configurator":
                            configurator = true;
                            break;
                        default:
                            MessageBox.Show(string.Format("Unknown argument \"{0}\".", arg), AppDomain.CurrentDomain.FriendlyName);
                            Application.Current.Shutdown(1);
                            return;
                    }
                }
                else
                {
                    playlistConfigFilePath = Path.GetFullPath(arg);
                }
            }

            if (configurator || playlistConfigFilePath == null)
            {
                RunConfigurator();
                return;
            }

            RunPlayer(playlistConfigFilePath);
        }

        private void RunConfigurator()
        {
            var configuratorWindow = new ConfiguratorWindow();
            configuratorWindow.RunPlaylistConfigEvent += ConfiguratorWindow_RunPlaylistConfig;
            configuratorWindow.Show();
        }

        private void ConfiguratorWindow_RunPlaylistConfig(object? sender, string e)
        {
            var configuratorWindow = (ConfiguratorWindow)sender!;
            RunPlayer(e);
            // Close last
            configuratorWindow.Close();
        }

        private void RunPlayer(string? playlistConfigFilePath)
        {
            Configuration.Configuration configuration;
            try
            {
                configuration = configurationService.LoadConfiguration(playlistConfigFilePath);
            }
            catch (IniIoException ex)
            {
                if (MessageBox.Show(Strings.WantToStartConfigurator, string.Format(Strings.ErrorReadingFile, ex.FilePath), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RunConfigurator();
                }
                else
                {
                    Application.Current.Shutdown(1);
                }
                return;
            }

            Configuration.UserConfiguration userConfiguration;
            try
            {
                userConfiguration = configurationService.LoadUserConfiguration();
            }
            catch (IniIoException ex)
            {
                Debug.WriteLine("Error reading user configuration ini file: " + ex.Message);
                userConfiguration = new(new IniDocument(), new Configuration.Parser());
            }

            Unosquare.FFME.Library.FFmpegDirectory = @"ffmpeg";
            var result = Unosquare.FFME.Library.LoadFFmpeg();
            Debug.WriteLine(result ? "FFmpeg loaded." : "FFmpeg NOT loaded.");

            mainModel = new MainModel(configuration, userConfiguration);

            if (configuration.Dof.Enabled)
            {
                dofMediator = new DofMediator(mainModel, configuration.Dof);
                dofMediator.Startup();
            }

            audioManager = new();
            var playFieldWindow = CreateWindow(mainModel, configuration.PlayField, audioManager);
            var backGlassWindow = CreateWindow(mainModel, configuration.BackGlass, audioManager);
            var dmdWindow = CreateWindow(mainModel, configuration.Dmd, audioManager);

            beaconController = new BeaconController(mainModel, beaconService, configurationService, dofMediator);
            _ = beaconController.Startup();
            appController = new AppController(mainModel);
            appController.Scan();
            displayController = new DisplayController(mainModel, audioManager);
            if (playFieldWindow != null)
            {
                displayController.ObserveWindow(playFieldWindow);
            }
            if (backGlassWindow != null)
            {
                displayController.ObserveWindow(backGlassWindow);
            }
            if (dmdWindow != null)
            {
                displayController.ObserveWindow(dmdWindow);
            }

            playFieldWindow?.Show();
            backGlassWindow?.Show();
            dmdWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (mainModel != null)
            {
                configurationService.SaveUserConfiguration(mainModel.UserConfiguration);
            }

            displayController?.Dispose();
            appController?.Dispose();
            audioManager?.Dispose();
            dofMediator?.Dispose();
        }

        private MainWindow? CreateWindow(MainModel mainModel, Configuration.Display displayConfig, AudioManager audioManager)
        {
            if (!displayConfig.Enabled)
            {
                return null;
            }
            var window = new MainWindow(mainModel, displayConfig, audioManager);
            return window;
        }
    }
}
