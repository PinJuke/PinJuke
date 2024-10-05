using PinJuke.Audio;
using PinJuke.Configurator;
using PinJuke.Controller;
using PinJuke.Dof;
using PinJuke.Ini;
using PinJuke.Model;
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
        private AppController? appController;
        private DisplayController? displayController;
        private DofMediator? dofMediator = null;
        private AudioManager? audioManager;

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
                configuration = LoadConfiguration(playlistConfigFilePath);
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
                userConfiguration = LoadUserConfiguration();
            }
            catch (IniIoException ex)
            {
                Debug.WriteLine("Error reading user configuration ini file: " + ex.Message);
                userConfiguration = new(new IniDocument());
            }

            Unosquare.FFME.Library.FFmpegDirectory = @"ffmpeg";
            var result = Unosquare.FFME.Library.LoadFFmpeg();
            Debug.WriteLine(result ? "FFmpeg loaded." : "FFmpeg NOT loaded.");

            mainModel = new MainModel(configuration, userConfiguration);

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

            SaveUserConfiguration();

            displayController?.Dispose();
            appController?.Dispose();
            audioManager?.Dispose();
            dofMediator?.Dispose();
        }

        private Configuration.Configuration LoadConfiguration(string? playlistConfigFilePath)
        {
            List<string> iniFilePaths = new();
            iniFilePaths.Add(Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH);
            if (playlistConfigFilePath != null)
            {
                iniFilePaths.Add(playlistConfigFilePath);
            }

            var loader = new Configuration.ConfigurationLoader();
            return loader.FromIniFilePaths(iniFilePaths, playlistConfigFilePath);
        }

        private Configuration.UserConfiguration LoadUserConfiguration()
        {
            var userConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PinJuke";
            var userConfigFile = Path.Combine(userConfigDir, Configuration.ConfigPath.USER_FILE_NAME);

            var loader = new Configuration.UserConfigurationLoader();
            return loader.FromIniFilePath(userConfigFile);
        }

        private void SaveUserConfiguration()
        {
            var userConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PinJuke";
            var userConfigFile = Path.Combine(userConfigDir, Configuration.ConfigPath.USER_FILE_NAME);

            Directory.CreateDirectory(userConfigDir);
            using var textWriter = new StreamWriter(userConfigFile);
            mainModel?.UserConfiguration.IniDocument.WriteTo(textWriter);
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
