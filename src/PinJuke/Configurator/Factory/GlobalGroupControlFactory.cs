using PinJuke.Configuration;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PinJuke.Configurator.Factory
{
    public class GlobalGroupControlFactory : GroupControlFactory
    {
        public const string MEDIA_PATH_CONTROL = "MediaPath";

        public GlobalGroupControlFactory(Parser parser, PinUpPlayerIniReader pinUpReader)
        {
            // Some cases of Key share the same value.
            var keys = Enum.GetNames<Key>().Select(name => new Item(name, Enum.Parse<Key>(name))).ToList();

            // Create controller button options (1-32 for typical gamepad controllers)
            var controllerButtons = new List<Item>();
            controllerButtons.Add(new Item("None", 0)); // Option for no button assigned
            for (int i = 1; i <= 32; i++)
            {
                controllerButtons.Add(new Item($"Button {i}", i));
            }

            LabelText = Strings.GlobalConfiguration;
            Controls = [
                new GroupControlFactory()
                {
                    LabelText = "PinJuke",
                    Controls = [
                        new RowFactory<PathControl>() {
                            LabelText = Strings.MediaPath,
                            ChildFactory = new PathControlFactory()
                            {
                                Name = MEDIA_PATH_CONTROL,
                                FileMode = false,
                                RelativeEnabled = true,
                                Converter = new PathConverter(parser, "PinJuke", "MediaPath"),
                            }
                        }
                    ]
                },
                new WindowGroupControlFactory(parser, "PlayField", false, pinUpReader, PinUpPlayerIniReader.PLAY_FIELD_SECTION_NAME)
                {
                    LabelText = Strings.DisplayPlayField,
                },
                new WindowGroupControlFactory(parser, "BackGlass", false, pinUpReader, PinUpPlayerIniReader.BACK_GLASS_SECTION_NAME)
                {
                    LabelText = Strings.DisplayBackGlass,
                },
                new WindowGroupControlFactory(parser, "DMD", true, pinUpReader, PinUpPlayerIniReader.DMD_SECTION_NAME)
                {
                    LabelText = Strings.DisplayDmd,
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Keyboard,
                    Controls = [
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyExit,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Exit"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyBrowse,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Browse"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyPrevious,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Previous"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyNext,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Next"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyPlayPause,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "PlayPause"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyVolumeDown,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeDown"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.KeyVolumeUp,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = keys,
                                Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeUp"),
                            }
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Controller,
                    Controls = [
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.ControllerExit,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = controllerButtons,
                                Converter = new ControllerSelectConverter(parser, "Controller", "Exit"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.ControllerBrowse,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = controllerButtons,
                                Converter = new ControllerSelectConverter(parser, "Controller", "Browse"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.ControllerPrevious,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = controllerButtons,
                                Converter = new ControllerSelectConverter(parser, "Controller", "Previous"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.ControllerNext,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = controllerButtons,
                                Converter = new ControllerSelectConverter(parser, "Controller", "Next"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.ControllerPlayPause,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = controllerButtons,
                                Converter = new ControllerSelectConverter(parser, "Controller", "PlayPause"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.ControllerVolumeDown,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = controllerButtons,
                                Converter = new ControllerSelectConverter(parser, "Controller", "VolumeDown"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.ControllerVolumeUp,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = controllerButtons,
                                Converter = new ControllerSelectConverter(parser, "Controller", "VolumeUp"),
                            }
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Milkdrop,
                    Controls = [
                        new RowFactory<PathControl>() {
                            LabelText = Strings.MilkdropPresetsPath,
                            ChildFactory = new PathControlFactory()
                            {
                                FileMode = false,
                                RelativeEnabled = true,
                                Converter = new PathConverter(parser, "Milkdrop", "PresetsPath"),
                            }
                        },
                        new RowFactory<PathControl>() {
                            LabelText = Strings.MilkdropTexturesPath,
                            ChildFactory = new PathControlFactory()
                            {
                                FileMode = false,
                                RelativeEnabled = true,
                                Converter = new PathConverter(parser, "Milkdrop", "TexturesPath"),
                            }
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Dof,
                    Controls = [
                        new RowFactory<BoolControl>() {
                            LabelText = Strings.Enable,
                            ChildFactory = new BoolControlFactory()
                            {
                                Converter = new BoolConverter(parser, "DOF", "Enabled"),
                            }
                        },
                        new RowFactory<PathControl>() {
                            LabelText = Strings.DofGlobalConfigFilePath,
                            ChildFactory = new PathControlFactory()
                            {
                                FileMode = true,
                                RelativeEnabled = false,
                                FileExtension = ".xml",
                                FileFilter = $"{Strings.XmlFile}|*.xml",
                                Converter = new PathConverter(parser, "DOF", "GlobalConfigFilePath"),
                            }
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = "Spotify",
                    Controls = [
                        new RowFactory<BoolControl>() {
                            LabelText = "Enable Spotify Integration",
                            ChildFactory = new BoolControlFactory()
                            {
                                Converter = new BoolConverter(parser, "Spotify", "Enabled"),
                                ChangedHandler = (ConfiguratorControl control) =>
                                {
                                    var enabled = ((BoolControl)control).Value;
                                    var group = control.GetParentGroup();
                                    
                                    // Safely update controls if they exist
                                    try
                                    {
                                        var clientIdControl = group.GetChildByName("SpotifyClientId") as TextControl;
                                        if (clientIdControl != null) clientIdControl.Enabled = enabled;
                                        
                                        var clientSecretControl = group.GetChildByName("SpotifyClientSecret") as TextControl;
                                        if (clientSecretControl != null) clientSecretControl.Enabled = enabled;
                                        
                                        var redirectUriControl = group.GetChildByName("SpotifyRedirectUri") as TextControl;
                                        if (redirectUriControl != null) redirectUriControl.Enabled = enabled;
                                        
                                        var usePreviewUrlsControl = group.GetChildByName("SpotifyUsePreviewUrls") as BoolControl;
                                        if (usePreviewUrlsControl != null) usePreviewUrlsControl.Enabled = enabled;
                                        
                                        var cacheDurationControl = group.GetChildByName("SpotifyCacheDuration") as NumberControl;
                                        if (cacheDurationControl != null) cacheDurationControl.Enabled = enabled;
                                        
                                        var maxTracksControl = group.GetChildByName("SpotifyMaxTracks") as NumberControl;
                                        if (maxTracksControl != null) maxTracksControl.Enabled = enabled;
                                        
                                        var authenticateButtonControl = group.GetChildByName("SpotifyAuthenticateButton") as ButtonControl;
                                        if (authenticateButtonControl != null) authenticateButtonControl.IsEnabled = enabled;
                                        
                                        var deviceSelectControl = group.GetChildByName("SpotifyDeviceSelect") as SelectControl;
                                        if (deviceSelectControl != null) deviceSelectControl.Enabled = enabled;
                                        
                                        var refreshDevicesButtonControl = group.GetChildByName("SpotifyRefreshDevicesButton") as ButtonControl;
                                        if (refreshDevicesButtonControl != null) refreshDevicesButtonControl.IsEnabled = enabled;
                                        
                                        var autoTransferPlaybackControl = group.GetChildByName("SpotifyAutoTransferPlayback") as BoolControl;
                                        if (autoTransferPlaybackControl != null) autoTransferPlaybackControl.Enabled = enabled;
                                        
                                        var defaultVolumeControl = group.GetChildByName("SpotifyDefaultVolume") as NumberControl;
                                        if (defaultVolumeControl != null) defaultVolumeControl.Enabled = enabled;
                                    }
                                    catch
                                    {
                                        // Controls may not exist yet, ignore
                                    }
                                },
                            }
                        },
                        new RowFactory<TextControl>() {
                            LabelText = "Spotify Client ID",
                            ChildFactory = new TextControlFactory()
                            {
                                Name = "SpotifyClientId",
                                Converter = new StringConverter(parser, "Spotify", "ClientId"),
                            }
                        },
                        new RowFactory<TextControl>() {
                            LabelText = "Spotify Client Secret",
                            ChildFactory = new TextControlFactory()
                            {
                                Name = "SpotifyClientSecret", 
                                Converter = new StringConverter(parser, "Spotify", "ClientSecret"),
                            }
                        },
                        new RowFactory<TextControl>() {
                            LabelText = "OAuth Redirect URI",
                            ChildFactory = new TextControlFactory()
                            {
                                Name = "SpotifyRedirectUri",
                                Converter = new StringConverter(parser, "Spotify", "RedirectUri"),
                            }
                        },
                        new RowFactory<BoolControl>() {
                            LabelText = "Use 30s Preview URLs (no Premium required)",
                            ChildFactory = new BoolControlFactory()
                            {
                                Name = "SpotifyUsePreviewUrls",
                                Converter = new BoolConverter(parser, "Spotify", "UsePreviewUrls"),
                            }
                        },
                        new RowFactory<NumberControl>() {
                            LabelText = "API Cache Duration (minutes)",
                            ChildFactory = new NumberControlFactory()
                            {
                                Name = "SpotifyCacheDuration",
                                Converter = new IntNumberConverter(parser, "Spotify", "CacheDurationMinutes"),
                            }
                        },
                        new RowFactory<NumberControl>() {
                            LabelText = "Max Tracks per Playlist",
                            ChildFactory = new NumberControlFactory()
                            {
                                Name = "SpotifyMaxTracks",
                                Converter = new IntNumberConverter(parser, "Spotify", "MaxTracksPerPlaylist"),
                            }
                        },
                        new RowFactory<ButtonControl>() {
                            LabelText = "Authentication",
                            ChildFactory = new ButtonControlFactory()
                            {
                                Name = "SpotifyAuthenticateButton",
                                Text = IsSpotifyAuthenticated() ? "Authenticated" : "Not Authenticated",
                                ClickHandler = async (control) => await AuthenticateSpotifyAsync(control),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = "Playback Device",
                            ChildFactory = new SelectControlFactory()
                            {
                                Name = "SpotifyDeviceSelect",
                                Converter = new StringSelectConverter(parser, "Spotify", "DeviceId"),
                                Items = GetInitialSpotifyDevices(),
                            }
                        },
                        new RowFactory<ButtonControl>() {
                            LabelText = "Device Management",
                            ChildFactory = new ButtonControlFactory()
                            {
                                Name = "SpotifyRefreshDevicesButton",
                                Text = "Refresh Devices",
                                ClickHandler = async (control) => await RefreshSpotifyDevicesAsync(control),
                            }
                        },
                        new RowFactory<BoolControl>() {
                            LabelText = "Auto-transfer playback to selected device (ensures playback happens on correct device)",
                            ChildFactory = new BoolControlFactory()
                            {
                                Name = "SpotifyAutoTransferPlayback",
                                Converter = new BoolConverter(parser, "Spotify", "AutoTransferPlayback"),
                            }
                        },
                        new RowFactory<NumberControl>() {
                            LabelText = "Default Spotify Volume (0-100, controls Spotify app volume)",
                            ChildFactory = new NumberControlFactory()
                            {
                                Name = "SpotifyDefaultVolume",
                                Converter = new IntNumberConverter(parser, "Spotify", "DefaultVolume"),
                            }
                        },
                    ]
                },
            ];
        }

        private async Task AuthenticateSpotifyAsync(ConfiguratorControl control)
        {
            try
            {
                var group = control.GetParentGroup();
                
                // Safely get control values
                var clientIdControl = group.GetChildByName("SpotifyClientId") as TextControl;
                var clientSecretControl = group.GetChildByName("SpotifyClientSecret") as TextControl;
                var redirectUriControl = group.GetChildByName("SpotifyRedirectUri") as TextControl;
                
                if (clientIdControl == null || clientSecretControl == null || redirectUriControl == null)
                {
                    System.Windows.MessageBox.Show("Authentication error: cannot return child. Found no control for \"SpotifyClientId\"", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
                
                var clientId = clientIdControl.Value;
                var clientSecret = clientSecretControl.Value;
                var redirectUri = redirectUriControl.Value;

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                {
                    System.Windows.MessageBox.Show("Please enter Client ID, Client Secret, and Redirect URI first.", "Spotify Authentication", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Create a temporary Spotify config and service for authentication
                var spotifyConfig = new Spotify.SpotifyConfig
                {
                    Enabled = true,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    RedirectUri = redirectUri
                };

                var spotifyService = new Spotify.SpotifyService();

                // Initialize and authenticate
                await spotifyService.InitializeAsync(spotifyConfig);
                var authResult = await spotifyService.AuthService.AuthenticateAsync();
                var success = authResult.IsSuccess;

                if (success)
                {
                    // Save authentication tokens to global config
                    SaveAuthenticationTokens(authResult);
                    
                    // Load user playlists to verify authentication worked
                    var playlists = await spotifyService.GetUserPlaylistsAsync();
                    System.Windows.MessageBox.Show($"Successfully authenticated with Spotify! Found {playlists.Count} playlists.\n\nDevices are loading automatically...\n\nNext steps:\n1. Select your preferred playback device from the dropdown\n2. Configure playlists in the playlist section", "Spotify Authentication", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    
                    // Update button text and auto-load devices
                    ((ButtonControl)control).Text = "Authenticated";
                    
                    // Try to auto-refresh devices with a delay to ensure UI is ready
                    try
                    {
                        var refreshButton = control.GetParentGroup().GetChildByName("SpotifyRefreshDevicesButton") as ButtonControl;
                        if (refreshButton != null)
                        {
                            // Use a background task with delay to ensure UI is fully loaded
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(1500); // Give UI time to settle
                                
                                // Execute refresh on UI thread
                                _ = System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                                {
                                    try
                                    {
                                        await RefreshSpotifyDevicesAsync(refreshButton);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Auto-refresh error: {ex.Message}");
                                    }
                                });
                            });
                        }
                    }
                    catch
                    {
                        // Ignore errors in auto-refresh
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show($"Authentication failed. {authResult.ErrorMessage}\n\nPlease check your credentials and try again.", "Spotify Authentication", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                
                spotifyService.Dispose();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Authentication error: {ex.Message}", "Spotify Authentication", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void SaveAuthenticationTokens(Spotify.SpotifyAuthResult authResult)
        {
            try
            {
                var globalConfigPath = Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var fullPath = System.IO.Path.GetFullPath(globalConfigPath);
                System.Diagnostics.Debug.WriteLine($"Saving tokens to: {fullPath}");
                
                var iniDoc = Ini.IniReader.TryRead(globalConfigPath) ?? new Ini.IniDocument();
                
                var spotifySection = iniDoc["Spotify"];
                spotifySection["AccessToken"] = authResult.AccessToken;
                spotifySection["RefreshToken"] = authResult.RefreshToken;
                spotifySection["ExpiresAt"] = authResult.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss");
                spotifySection["Scopes"] = string.Join(",", authResult.Scopes);
                
                // Ensure directory exists
                var directory = System.IO.Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                // Save the updated configuration
                using var textWriter = new System.IO.StreamWriter(globalConfigPath);
                iniDoc.WriteTo(textWriter);
                
                System.Diagnostics.Debug.WriteLine($"Successfully saved Spotify auth tokens");
                System.Diagnostics.Debug.WriteLine($"AccessToken: {authResult.AccessToken?.Substring(0, Math.Min(20, authResult.AccessToken.Length))}...");
                System.Diagnostics.Debug.WriteLine($"Expires: {authResult.ExpiresAt}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving authentication tokens: {ex.Message}");
                System.Windows.MessageBox.Show($"Warning: Could not save authentication tokens. You may need to re-authenticate next time.\n\nError: {ex.Message}", "Save Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private bool IsSpotifyAuthenticated()
        {
            try
            {
                var globalConfigPath = Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var fullPath = System.IO.Path.GetFullPath(globalConfigPath);
                System.Diagnostics.Debug.WriteLine($"Checking auth from: {fullPath}");
                
                var iniDoc = Ini.IniReader.TryRead(globalConfigPath) ?? new Ini.IniDocument();
                
                var spotifySection = iniDoc["Spotify"];
                var accessToken = spotifySection["AccessToken"];
                var expiresAtStr = spotifySection["ExpiresAt"];
                
                System.Diagnostics.Debug.WriteLine($"Checking auth - AccessToken: {(!string.IsNullOrEmpty(accessToken) ? "Present" : "Missing")}");
                System.Diagnostics.Debug.WriteLine($"Checking auth - ExpiresAt: {expiresAtStr}");
                
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(expiresAtStr))
                {
                    return false;
                }
                
                if (DateTime.TryParse(expiresAtStr, out var expiresAt))
                {
                    var isValid = DateTime.Now < expiresAt;
                    System.Diagnostics.Debug.WriteLine($"Token valid: {isValid} (expires {expiresAt}, now {DateTime.Now})");
                    return isValid;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking auth: {ex.Message}");
                return false;
            }
        }

        private List<Item> GetSpotifyDevices()
        {
            // Return initial empty list or cached devices
            return new List<Item> { new Item("(Click refresh to load devices)", "") };
        }

        private List<Item> GetInitialSpotifyDevices()
        {
            var items = new List<Item> { new Item("(No device selected)", "") };
            
            // Try to include previously saved device if available
            try
            {
                var globalConfigPath = Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var iniDoc = Ini.IniReader.TryRead(globalConfigPath);
                if (iniDoc != null)
                {
                    var spotifySection = iniDoc["Spotify"];
                    var savedDeviceId = spotifySection?["DeviceId"];
                    var savedDeviceName = spotifySection?["DeviceName"];
                    
                    if (!string.IsNullOrEmpty(savedDeviceId) && !string.IsNullOrEmpty(savedDeviceName))
                    {
                        items.Add(new Item($"{savedDeviceName} (saved)", savedDeviceId));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading saved device: {ex.Message}");
            }
            
            // Try to load devices automatically if authenticated
            if (IsSpotifyAuthenticated())
            {
                items.Add(new Item("(Loading devices...)", ""));
            }
            else
            {
                items.Add(new Item("(Authenticate first to see devices)", ""));
            }
            
            return items;
        }



        private async Task RefreshSpotifyDevicesAsync(ConfiguratorControl control)
        {
            try
            {
                if (!IsSpotifyAuthenticated())
                {
                    System.Windows.MessageBox.Show("Please authenticate with Spotify first before loading devices.", "Device Selection", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Load authentication from global config
                var authResult = LoadSpotifyAuthentication();
                if (authResult == null || authResult.IsExpired)
                {
                    System.Windows.MessageBox.Show("Spotify authentication has expired. Please re-authenticate first.", "Device Selection", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Get the group and config values
                var group = control.GetParentGroup();
                
                // Safely get control values
                var clientIdControl = group.GetChildByName("SpotifyClientId") as TextControl;
                var clientSecretControl = group.GetChildByName("SpotifyClientSecret") as TextControl;
                var redirectUriControl = group.GetChildByName("SpotifyRedirectUri") as TextControl;
                
                if (clientIdControl == null || clientSecretControl == null || redirectUriControl == null)
                {
                    System.Windows.MessageBox.Show("Error loading devices: Cannot return child. Found no control for \"SpotifyClientId\"", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return;
                }
                
                var clientId = clientIdControl.Value;
                var clientSecret = clientSecretControl.Value;
                var redirectUri = redirectUriControl.Value;

                // Create temporary config and media provider
                var config = new Spotify.SpotifyConfig
                {
                    Enabled = true,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    RedirectUri = redirectUri
                };

                var mediaProvider = new Spotify.SpotifyMediaProvider(config);
                mediaProvider.SetAuthResult(authResult);

                // Get available devices
                var devices = await mediaProvider.GetAvailableDevicesAsync();
                
                var deviceItems = new List<Item> { new Item("(No device selected)", "") };
                
                foreach (var device in devices)
                {
                    deviceItems.Add(new Item(device.DisplayName, device.Id));
                }

                if (deviceItems.Count == 1)
                {
                    deviceItems.Add(new Item("(No devices found - make sure Spotify app is running)", ""));
                }

                // Update the device select control
                var deviceSelect = (SelectControl)group.GetChildByName("SpotifyDeviceSelect");
                var currentSelection = deviceSelect.SelectedValue?.ToString();
                deviceSelect.Items = deviceItems;

                // Restore previous selection if still available, otherwise default to first item
                if (!string.IsNullOrEmpty(currentSelection))
                {
                    var stillAvailable = deviceItems.Any(item => item.Value?.ToString() == currentSelection);
                    if (stillAvailable)
                    {
                        deviceSelect.SelectedValue = currentSelection;
                    }
                    else
                    {
                        // Previous selection no longer available, select default
                        deviceSelect.SelectedIndex = 0; // "(No device selected)"
                    }
                }
                else
                {
                    // No previous selection, select default
                    deviceSelect.SelectedIndex = 0; // "(No device selected)"
                }

                // Save device name for the currently selected device if any
                var finalSelection = deviceSelect.SelectedValue?.ToString();
                if (!string.IsNullOrEmpty(finalSelection))
                {
                    var selectedDevice = devices.FirstOrDefault(d => d.Id == finalSelection);
                    if (selectedDevice != null)
                    {
                        SaveDeviceName(selectedDevice.Id, selectedDevice.Name);
                    }
                }

                mediaProvider.Dispose();

                if (devices.Count > 0)
                {
                    System.Windows.MessageBox.Show($"Found {devices.Count} available Spotify devices. Make sure to select your preferred playback device.", "Device Refresh", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("No Spotify devices found. Please make sure:\n\n1. Spotify app is running on this computer or another device\n2. You're logged into the same Spotify account\n3. Your devices are connected to the internet", "No Devices Found", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading devices: {ex.Message}\n\nTip: Make sure Spotify app is running and you're logged in.", "Device Selection", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private Spotify.SpotifyAuthResult? LoadSpotifyAuthentication()
        {
            try
            {
                var globalConfigPath = Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var iniDoc = Ini.IniReader.TryRead(globalConfigPath);
                if (iniDoc == null) return null;

                var spotifySection = iniDoc["Spotify"];
                if (spotifySection == null) return null;

                var parser = new Configuration.Parser();
                var accessToken = parser.ParseString(spotifySection["AccessToken"]);
                var refreshToken = parser.ParseString(spotifySection["RefreshToken"]);
                var expiresAtStr = parser.ParseString(spotifySection["ExpiresAt"]);
                var scopesStr = parser.ParseString(spotifySection["Scopes"]);

                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }

                // Parse expiration date
                DateTime expiresAt = DateTime.Now.AddDays(-1); // Default to expired
                if (!string.IsNullOrEmpty(expiresAtStr) && DateTime.TryParse(expiresAtStr, out var parsedDate))
                {
                    expiresAt = parsedDate;
                }

                return new Spotify.SpotifyAuthResult
                {
                    IsSuccess = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken ?? "",
                    ExpiresAt = expiresAt,
                    Scopes = !string.IsNullOrEmpty(scopesStr) ? scopesStr.Split(',') : Array.Empty<string>()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading authentication: {ex.Message}");
                return null;
            }
        }

        private void SaveDeviceName(string deviceId, string deviceName)
        {
            try
            {
                var globalConfigPath = Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var iniDoc = Ini.IniReader.TryRead(globalConfigPath) ?? new Ini.IniDocument();
                
                var spotifySection = iniDoc["Spotify"];
                spotifySection["DeviceName"] = deviceName;
                
                // Ensure directory exists
                var directory = System.IO.Path.GetDirectoryName(globalConfigPath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                // Save the updated configuration
                using var textWriter = new System.IO.StreamWriter(globalConfigPath);
                iniDoc.WriteTo(textWriter);
                
                System.Diagnostics.Debug.WriteLine($"Saved device name: {deviceName} for device: {deviceId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving device name: {ex.Message}");
            }
        }
    }

    public class WindowGroupControlFactory : GroupControlFactory
    {
        public WindowGroupControlFactory(Parser parser, string sectionName, bool enabledAvailable, PinUpPlayerIniReader pinUpReader, string pinUpSectionName)
        {
            ControlFactory<UIElement>[] enabledControls = enabledAvailable
                ? [
                    new RowFactory<BoolControl>() {
                        LabelText = Strings.Enable,
                        ChildFactory = new BoolControlFactory()
                        {
                            Converter = new BoolConverter(parser, sectionName, "Enabled"),
                        }
                    },
                ]
                : [];

            Controls = [
                ..enabledControls,
                new RowFactory<ButtonControl>() {
                    LabelText = "",
                    ChildFactory = new ButtonControlFactory()
                    {
                        Text = Strings.GetDisplayPositionFromPinup,
                        ClickHandler = (buttonControl) =>
                        {
                            (int, int, int, int)? position;
                            try
                            {
                                position = pinUpReader.FindPosition(pinUpSectionName);
                            }
                            catch (IniIoException ex)
                            {
                                MessageBox.Show(string.Format(Strings.ErrorReadingFile, ex.FilePath), AppDomain.CurrentDomain.FriendlyName);
                                return;
                            }
                            if (position == null)
                            {
                                MessageBox.Show(string.Format(Strings.PathNotFound, PinUpPlayerIniReader.BALLER_PIN_UP_PLAYER_INI), AppDomain.CurrentDomain.FriendlyName);
                                return;
                            }
                            var group = buttonControl.GetParentGroup();
                            ((NumberControl)group.GetChildByName("WindowLeft")).Value = position.Value.Item1;
                            ((NumberControl)group.GetChildByName("WindowTop")).Value = position.Value.Item2;
                            ((NumberControl)group.GetChildByName("WindowWidth")).Value = position.Value.Item3;
                            ((NumberControl)group.GetChildByName("WindowHeight")).Value = position.Value.Item4;
                        },
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.RectLeft,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowLeft",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowLeft"),
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.RectTop,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowTop",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowTop"),
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.RectWidth,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowWidth",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowWidth"),
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.RectHeight,
                    ChildFactory = new NumberControlFactory()
                    {
                        Name = "WindowHeight",
                        Converter = new IntNumberConverter(parser, sectionName, "WindowHeight"),
                    }
                },
                new RowFactory<NumberControl>() {
                    LabelText = Strings.Scale,
                    ChildFactory = new NumberControlFactory()
                    {
                        Converter = new FloatNumberConverter(parser, sectionName, "ContentScale"),
                    }
                },
                new RowFactory<SelectControl>() {
                    LabelText = Strings.Rotation,
                    ChildFactory = new SelectControlFactory()
                    {
                        Items = new()
                        {
                            new("-90 °", -90),
                            new("0 °", 0),
                            new("90 °", 90),
                            new("180 °", 180),
                        },
                        Converter = new IntSelectConverter(parser, sectionName, "ContentRotation"),
                    }
                },
            ];
        }
    }
}
