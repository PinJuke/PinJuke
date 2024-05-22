using PinJuke.Audio;
using PinJuke.Controller;
using PinJuke.Dof;
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
        private DofMediator? dofMediator = null;
        private AudioManager? audioManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            string? playlistConfigFilePath = null;
            if (e.Args.Length >= 1)
            {
                playlistConfigFilePath = Path.GetFullPath(e.Args[0]);
            }

            Configuration.Configuration configuration;
            try
            {
                configuration = LoadConfiguration(playlistConfigFilePath);
            }
            catch (Configuration.IniIoException ex)
            {
                MessageBox.Show(ex.Message, AppDomain.CurrentDomain.FriendlyName);
                Application.Current.Shutdown(1);
                return;
            }

            Configuration.UserConfiguration userConfiguration;
            try
            {
                userConfiguration = LoadUserConfiguration();
            }
            catch (Configuration.IniIoException ex)
            {
                Debug.WriteLine("Error reading user configuration ini file: " + ex.Message);
                userConfiguration = new(new Configuration.IniDocument());
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

            playFieldWindow?.Show();
            backGlassWindow?.Show();
            dmdWindow?.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            SaveUserConfiguration();

            appController?.Dispose();
            audioManager?.Dispose();
            dofMediator?.Dispose();
        }

        private Configuration.Configuration LoadConfiguration(string? playlistConfigFilePath)
        {
            List<string> iniFilePaths = new();
            iniFilePaths.Add(@"Configs\PinJuke.global.ini");
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
            var userConfigFile = userConfigDir + @"\PinJuke.user.ini";

            var loader = new Configuration.UserConfigurationLoader();
            return loader.FromIniFilePath(userConfigFile);
        }

        private void SaveUserConfiguration()
        {
            var userConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PinJuke";
            var userConfigFile = userConfigDir + @"\PinJuke.user.ini";

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
            var controller = new DisplayController(mainModel, window, audioManager);
            return window;
        }
    }
}
