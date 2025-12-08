using PinJuke.Audio;
using PinJuke.Configurator;
using PinJuke.Controller;
using PinJuke.Dof;
using PinJuke.Ini;
using PinJuke.Model;
using PinJuke.Onboarding;
using PinJuke.Service;
using PinJuke.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke
{
    public partial class App : Application
    {
        private MainModel? mainModel;
        private ConfigurationService configurationService = new();
        private BeaconService beaconService = new();
        private UpdateCheckService updateCheckService = new();
        private BeaconController? beaconController = null;
        private AppController? appController = null;
        private DisplayController? displayController = null;
        private DofMediator? dofMediator = null;
        private AudioManager? audioManager = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // For testing:
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

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
                            UiUtil.ShowErrorMessage(string.Format("Unknown argument \"{0}\".", arg));
                            Application.Current.Shutdown(1);
                            return;
                    }
                }
                else
                {
                    playlistConfigFilePath = arg;
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
            var userConfiguration = GetUserConfiguration();
            if (!userConfiguration.SetUp)
            {
                RunOnboarding();
                return;
            }

            RunConfiguratorNow();
        }

        private void RunOnboarding()
        {
            var onboardingWindow = new OnboardingWindow(beaconService, configurationService);
            onboardingWindow.FinishEvent += OnboardingWindow_Finish;
            onboardingWindow.Show();
        }

        private void OnboardingWindow_Finish(object? sender, FinishEventData e)
        {
            var window = (OnboardingWindow)sender!;
            var configuratorWindow = RunConfiguratorNow();
            // Close last
            window.Close();

            if (e.CreatePlaylist)
            {
                configuratorWindow.ShowAddPlaylist();
            }
        }

        private ConfiguratorWindow RunConfiguratorNow()
        {
            var userConfiguration = GetUserConfiguration();
            var configuratorWindow = new ConfiguratorWindow(updateCheckService, configurationService);
            configuratorWindow.RunPlaylistConfigEvent += ConfiguratorWindow_RunPlaylistConfig;
            configuratorWindow.RunOnboardingEvent += ConfiguratorWindow_RunOnboarding;
            configuratorWindow.Show();
            if (userConfiguration.UpdateCheckEnabled == true)
            {
                configuratorWindow.CheckForUpdates();
            }
            return configuratorWindow;
        }

        private void ConfiguratorWindow_RunPlaylistConfig(object? sender, string e)
        {
            var window = (ConfiguratorWindow)sender!;
            RunPlayer(e);
            // Close last
            window.Close();
        }

        private void ConfiguratorWindow_RunOnboarding(object? sender, EventArgs eventArgs)
        {
            var window = (ConfiguratorWindow)sender!;
            RunOnboarding();
            // Close last
            window.Close();
        }

        private void RunPlayer(string? playlistConfigFilePath)
        {
            if (playlistConfigFilePath != null)
            {
                playlistConfigFilePath = Path.GetFullPath(playlistConfigFilePath);
            }

            var userConfiguration = GetUserConfiguration();
            Configuration.Configuration configuration;
            try
            {
                configuration = GetConfiguration(playlistConfigFilePath);
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
            mainModel = new MainModel(configuration, userConfiguration);

            Unosquare.FFME.Library.FFmpegDirectory = @"ffmpeg";
            var result = Unosquare.FFME.Library.LoadFFmpeg();
            Debug.WriteLine(result ? "FFmpeg loaded." : "FFmpeg NOT loaded.");

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
            if (dmdWindow != null && backGlassWindow != null)
            {
                dmdWindow.Owner = backGlassWindow;
            }
            dmdWindow?.Show();
        }

        private Configuration.UserConfiguration GetUserConfiguration()
        {
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
            return userConfiguration;
        }

        private Configuration.Configuration GetConfiguration(string? playlistConfigFilePath)
        {
            return configurationService.LoadConfiguration(playlistConfigFilePath);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (mainModel != null)
            {
                try
                {
                    configurationService.SaveUserConfiguration(mainModel.UserConfiguration);
                }
                catch (IOException ex)
                {
                    Debug.WriteLine("Error writing user configuration ini file: " + ex.Message);
                }
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
