using PinJuke.Audio;
using PinJuke.Configurator;
using PinJuke.Controller;
using PinJuke.Dof;
using PinJuke.Ini;
using PinJuke.Model;
using PinJuke.Onboarding;
using PinJuke.Service;
using PinJuke.Spotify;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PinJuke
{
    public partial class App : Application
    {
        // Console debugging disabled for production
        // [DllImport("kernel32.dll")]
        // private static extern bool AllocConsole();
        
        private MainModel? mainModel;
        private ConfigurationService configurationService = new();
        private BeaconService beaconService = new();
        private UpdateCheckService updateCheckService = new();
        private BeaconController? beaconController = null;
        private AppController? appController = null;
        private DisplayController? displayController = null;
        private DofMediator? dofMediator = null;
        private AudioManager? audioManager = null;
        private SpotifyIntegrationService? spotifyIntegration = null;

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
                            MessageBox.Show(string.Format("Unknown argument \"{0}\".", arg), AppDomain.CurrentDomain.FriendlyName);
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

        private async void RunPlayer(string? playlistConfigFilePath)
        {
            // Console debugging disabled for production
            // AllocConsole();
            // var consoleListener = new ConsoleTraceListener();
            // System.Diagnostics.Trace.Listeners.Add(consoleListener);
            // Console.WriteLine("=== PinJuke Debug Console ===");
            // Debug.WriteLine("Debug console initialized");
            
            try
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
                
                Debug.WriteLine($"Player configuration - SourceType: {configuration.Player.SourceType}, SpotifyPlaylistId: {configuration.Player.SpotifyPlaylistId}");
                
                mainModel = new MainModel(configuration, userConfiguration);
                
                // Subscribe to pre-shutdown event to handle Spotify pause before windows close
                mainModel.PreShutdownEvent += MainModel_PreShutdownEvent;

                Unosquare.FFME.Library.FFmpegDirectory = @"ffmpeg";
                var result = Unosquare.FFME.Library.LoadFFmpeg();
                Debug.WriteLine(result ? "FFmpeg loaded." : "FFmpeg NOT loaded.");

                if (configuration.Dof.Enabled)
                {
                    dofMediator = new DofMediator(mainModel, configuration.Dof);
                    dofMediator.Startup();
                }
                // Initialize Spotify integration if enabled
                SimpleLogger.Log($"Spotify.Enabled: {configuration.Spotify.Enabled}");
                SimpleLogger.Log($"Spotify.ClientId: '{configuration.Spotify.ClientId}'");
                SimpleLogger.Log($"Spotify.RedirectUri: '{configuration.Spotify.RedirectUri}'");
                
                if (configuration.Spotify.Enabled)
                {
                    try
                    {
                        SimpleLogger.Log("Spotify integration is enabled. Initializing...");
                        Debug.WriteLine("Spotify integration is enabled. Initializing...");
                        Debug.WriteLine($"Spotify ClientId: {configuration.Spotify.ClientId}");
                        Debug.WriteLine($"Spotify RedirectUri: {configuration.Spotify.RedirectUri}");
                        
                        SimpleLogger.Log("Creating SpotifyIntegrationService...");
                        spotifyIntegration = new SpotifyIntegrationService(configuration);
                        
                        SimpleLogger.Log("Calling InitializeAsync...");
                        await spotifyIntegration.InitializeAsync(mainModel);
                        
                        SimpleLogger.Log("Creating SpotifyMediaController...");
                        // Register Spotify as a media controller
                        var spotifyController = new SpotifyMediaController(spotifyIntegration, () => mainModel.GetCurrentPlaylist());
                        
                        SimpleLogger.Log("Registering media controller...");
                        mainModel.RegisterMediaController(spotifyController);
                        
                        SimpleLogger.Log("Spotify integration initialization completed.");
                        Debug.WriteLine("Spotify integration initialization completed.");
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.Log($"Error initializing Spotify integration: {ex.Message}");
                        SimpleLogger.Log($"Exception details: {ex}");
                        Debug.WriteLine($"Error initializing Spotify integration: {ex.Message}");
                        Debug.WriteLine($"Exception details: {ex}");
                        // Continue without Spotify integration
                        spotifyIntegration = null;
                    }
                }
                else
                {
                    SimpleLogger.Log("Spotify integration is disabled in configuration!");
                    Debug.WriteLine("Spotify integration is disabled.");
                }

                // Initialize all registered media controllers
                try
                {
                    SimpleLogger.Log("Initializing media controllers...");
                    await mainModel.InitializeMediaControllersAsync();
                    SimpleLogger.Log("Media controllers initialized.");
                }
                catch (Exception ex)
                {
                    SimpleLogger.Log($"Error initializing media controllers: {ex.Message}");
                    SimpleLogger.Log($"Exception details: {ex}");
                    Debug.WriteLine($"Error initializing media controllers: {ex.Message}");
                    Debug.WriteLine($"Exception details: {ex}");
                }

                audioManager = new();
                var playFieldWindow = CreateWindow(mainModel, configuration.PlayField, audioManager, spotifyIntegration?.MediaProvider, spotifyIntegration);
                var backGlassWindow = CreateWindow(mainModel, configuration.BackGlass, audioManager, spotifyIntegration?.MediaProvider, spotifyIntegration);
                var dmdWindow = CreateWindow(mainModel, configuration.Dmd, audioManager, spotifyIntegration?.MediaProvider, spotifyIntegration);

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
            catch (Exception ex)
            {
                Debug.WriteLine($"Critical error starting PinJuke player: {ex}");
                MessageBox.Show($"Failed to start PinJuke player:\n\n{ex.Message}\n\nCheck the debug output for more details.", "PinJuke Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }
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

        private void MainModel_PreShutdownEvent(object? sender, EventArgs e)
        {
            // Handle Spotify pause when ESC is pressed (before windows close)
            if (spotifyIntegration?.IsEnabled == true)
            {
                try
                {
                    var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SPOTIFY ESC] Triggering Spotify pause (fire-and-forget)...";
                    Console.WriteLine(logMessage);
                    Debug.WriteLine("App.PreShutdown: Triggering Spotify pause");
                    
                    // Write to log file for debugging
                    try
                    {
                        File.AppendAllText("pinjuke-spotify-debug.log", logMessage + Environment.NewLine);
                    }
                    catch { /* Ignore file logging errors */ }
                    
                    // Fire-and-forget approach - trigger the pause but don't wait for it
                    // This avoids blocking the shutdown process while still sending the pause command
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var pauseResult = await spotifyIntegration.PlaybackController.PauseAsync();
                            var resultMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SPOTIFY ESC] Background pause result: {pauseResult}";
                            
                            // Try to log the result, but don't rely on it since app might be closing
                            try
                            {
                                File.AppendAllText("pinjuke-spotify-debug.log", resultMessage + Environment.NewLine);
                            }
                            catch { /* App might be closing, ignore logging errors */ }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SPOTIFY ESC] Background pause error: {ex.Message}";
                                File.AppendAllText("pinjuke-spotify-debug.log", errorMessage + Environment.NewLine);
                            }
                            catch { /* App might be closing, ignore logging errors */ }
                        }
                    });
                    
                    var triggerMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SPOTIFY ESC] Pause command triggered, continuing shutdown...";
                    Console.WriteLine(triggerMessage);
                    
                    try
                    {
                        File.AppendAllText("pinjuke-spotify-debug.log", triggerMessage + Environment.NewLine);
                    }
                    catch { /* Ignore file logging errors */ }
                }
                catch (Exception ex)
                {
                    var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SPOTIFY ESC] Error triggering Spotify pause: {ex.Message}";
                    Console.WriteLine(errorMessage);
                    Debug.WriteLine($"App.PreShutdown: Error triggering Spotify pause: {ex.Message}");
                    
                    try
                    {
                        File.AppendAllText("pinjuke-spotify-debug.log", errorMessage + Environment.NewLine);
                    }
                    catch { /* Ignore file logging errors */ }
                }
            }
            else
            {
                var statusMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [SPOTIFY ESC] Spotify integration not enabled - Enabled: {spotifyIntegration?.IsEnabled}";
                Console.WriteLine(statusMessage);
                try
                {
                    File.AppendAllText("pinjuke-spotify-debug.log", statusMessage + Environment.NewLine);
                }
                catch { /* Ignore file logging errors */ }
            }
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

            // Pause Spotify playback on app exit if integration is active
            if (spotifyIntegration?.IsEnabled == true && spotifyIntegration.IsConnected)
            {
                try
                {
                    Debug.WriteLine("App.OnExit: Pausing Spotify playback before app exit");
                    
                    // Pause Spotify asynchronously but don't wait (to avoid blocking shutdown)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var pauseResult = await spotifyIntegration.MediaProvider.PauseAsync();
                            Debug.WriteLine($"App.OnExit: Spotify pause result: {pauseResult}");
                        }
                        catch (Exception spotifyEx)
                        {
                            Debug.WriteLine($"App.OnExit: Error pausing Spotify: {spotifyEx.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"App.OnExit: Exception while attempting to pause Spotify: {ex.Message}");
                }
            }

            // Unsubscribe from events
            if (mainModel != null)
            {
                mainModel.PreShutdownEvent -= MainModel_PreShutdownEvent;
            }

            displayController?.Dispose();
            appController?.Dispose();
            audioManager?.Dispose();
            dofMediator?.Dispose();
            spotifyIntegration?.Dispose();
        }

        private MainWindow? CreateWindow(MainModel mainModel, Configuration.Display displayConfig, AudioManager audioManager, SpotifyMediaProvider? spotifyMediaProvider = null, SpotifyIntegrationService? spotifyIntegrationService = null)
        {
            if (!displayConfig.Enabled)
            {
                return null;
            }
            var window = new MainWindow(mainModel, displayConfig, audioManager, spotifyMediaProvider, spotifyIntegrationService);
            return window;
        }
    }
}
