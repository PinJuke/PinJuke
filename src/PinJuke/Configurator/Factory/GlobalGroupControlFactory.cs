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
                        new PathControlFactory()
                        {
                            LabelText = Strings.MediaPath,
                            Name = MEDIA_PATH_CONTROL,
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "PinJuke", "MediaPath"),
                        },
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
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyExit,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Exit"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyBrowse,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Browse"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyPrevious,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Previous"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyNext,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "Next"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyPlayPause,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "PlayPause"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyVolumeDown,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeDown"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.KeyVolumeUp,
                            Items = keys,
                            Converter = new EnumSelectConverter<Key>(parser, "Keyboard", "VolumeUp"),
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Controller,
                    Controls = [
                        new SelectControlFactory()
                        {
                            LabelText = Strings.ControllerExit,
                            Items = controllerButtons,
                            Converter = new ControllerSelectConverter(parser, "Controller", "Exit"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.ControllerBrowse,
                            Items = controllerButtons,
                            Converter = new ControllerSelectConverter(parser, "Controller", "Browse"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.ControllerPrevious,
                            Items = controllerButtons,
                            Converter = new ControllerSelectConverter(parser, "Controller", "Previous"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.ControllerNext,
                            Items = controllerButtons,
                            Converter = new ControllerSelectConverter(parser, "Controller", "Next"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.ControllerPlayPause,
                            Items = controllerButtons,
                            Converter = new ControllerSelectConverter(parser, "Controller", "PlayPause"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.ControllerVolumeDown,
                            Items = controllerButtons,
                            Converter = new ControllerSelectConverter(parser, "Controller", "VolumeDown"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.ControllerVolumeUp,
                            Items = controllerButtons,
                            Converter = new ControllerSelectConverter(parser, "Controller", "VolumeUp"),
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Milkdrop,
                    Controls = [
                        new PathControlFactory()
                        {
                            LabelText = Strings.MilkdropPresetsPath,
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "Milkdrop", "PresetsPath"),
                        },
                        new PathControlFactory()
                        {
                            LabelText = Strings.MilkdropTexturesPath,
                            FileMode = false,
                            RelativeEnabled = true,
                            Converter = new PathConverter(parser, "Milkdrop", "TexturesPath"),
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = Strings.Dof,
                    Controls = [
                        new BoolControlFactory()
                        {
                            LabelText = Strings.Enable,
                            Converter = new BoolConverter(parser, "DOF", "Enabled"),
                        },
                        new PathControlFactory()
                        {
                            LabelText = Strings.DofGlobalConfigFilePath,
                            FileMode = true,
                            RelativeEnabled = false,
                            FileExtension = ".xml",
                            FileFilter = $"{Strings.XmlFile}|*.xml",
                            Converter = new PathConverter(parser, "DOF", "GlobalConfigFilePath"),
                        },
                    ]
                },
                new GroupControlFactory()
                {
                    LabelText = "Spotify",
                    Controls = [
                        new BoolControlFactory()
                        {
                            LabelText = "Enable Spotify Integration",
                            Converter = new BoolConverter(parser, "Spotify", "Enabled"),
                            ChangedHandler = (ConfiguratorControl control) =>
                            {
                                var enabled = ((BoolControl)control).Value;
                                var group = control.GetParentGroup();
                                ((TextControl)group.GetChildByName("SpotifyClientId")).Enabled = enabled;
                                ((TextControl)group.GetChildByName("SpotifyClientSecret")).Enabled = enabled;
                                ((TextControl)group.GetChildByName("SpotifyRedirectUri")).Enabled = enabled;
                                ((BoolControl)group.GetChildByName("SpotifyUsePreviewUrls")).Enabled = enabled;
                                ((NumberControl)group.GetChildByName("SpotifyCacheDuration")).Enabled = enabled;
                                ((NumberControl)group.GetChildByName("SpotifyMaxTracks")).Enabled = enabled;
                                ((ButtonControl)group.GetChildByName("SpotifyAuthenticateButton")).IsEnabled = enabled;
                                ((SelectControl)group.GetChildByName("SpotifyDeviceSelect")).Enabled = enabled;
                                ((ButtonControl)group.GetChildByName("SpotifyRefreshDevicesButton")).IsEnabled = enabled;
                                ((BoolControl)group.GetChildByName("SpotifyAutoTransferPlayback")).Enabled = enabled;
                                ((NumberControl)group.GetChildByName("SpotifyDefaultVolume")).Enabled = enabled;
                            },
                        },
                        new TextControlFactory()
                        {
                            Name = "SpotifyClientId",
                            LabelText = "Spotify Client ID",
                            Converter = new StringConverter(parser, "Spotify", "ClientId"),
                        },
                        new TextControlFactory()
                        {
                            Name = "SpotifyClientSecret", 
                            LabelText = "Spotify Client Secret",
                            Converter = new StringConverter(parser, "Spotify", "ClientSecret"),
                        },
                        new TextControlFactory()
                        {
                            Name = "SpotifyRedirectUri",
                            LabelText = "OAuth Redirect URI",
                            Converter = new StringConverter(parser, "Spotify", "RedirectUri"),
                        },
                        new BoolControlFactory()
                        {
                            Name = "SpotifyUsePreviewUrls",
                            LabelText = "Use 30s Preview URLs (no Premium required)",
                            Converter = new BoolConverter(parser, "Spotify", "UsePreviewUrls"),
                        },
                        new NumberControlFactory()
                        {
                            Name = "SpotifyCacheDuration",
                            LabelText = "API Cache Duration (minutes)",
                            Converter = new IntNumberConverter(parser, "Spotify", "CacheDurationMinutes"),
                        },
                        new NumberControlFactory()
                        {
                            Name = "SpotifyMaxTracks",
                            LabelText = "Max Tracks per Playlist",
                            Converter = new IntNumberConverter(parser, "Spotify", "MaxTracksPerPlaylist"),
                        },
                        new ButtonControlFactory()
                        {
                            Name = "SpotifyAuthenticateButton",
                            Text = IsSpotifyAuthenticated() ? "Authenticated" : "Not Authenticated",
                            ClickHandler = async (control) => await AuthenticateSpotifyAsync(control),
                        },
                        new SelectControlFactory()
                        {
                            Name = "SpotifyDeviceSelect",
                            LabelText = "Playback Device",
                            Converter = new StringSelectConverter(parser, "Spotify", "DeviceId"),
                            Items = GetInitialSpotifyDevices(),
                        },
                        new ButtonControlFactory()
                        {
                            Name = "SpotifyRefreshDevicesButton",
                            Text = "Refresh Devices",
                            ClickHandler = async (control) => await RefreshSpotifyDevicesAsync(control),
                        },
                        new BoolControlFactory()
                        {
                            Name = "SpotifyAutoTransferPlayback",
                            LabelText = "Auto-transfer playback to selected device (ensures playback happens on correct device)",
                            Converter = new BoolConverter(parser, "Spotify", "AutoTransferPlayback"),
                        },
                        new NumberControlFactory()
                        {
                            Name = "SpotifyDefaultVolume",
                            LabelText = "Default Spotify Volume (0-100, controls Spotify app volume)",
                            Converter = new IntNumberConverter(parser, "Spotify", "DefaultVolume"),
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
                var clientId = ((TextControl)group.GetChildByName("SpotifyClientId")).Value;
                var clientSecret = ((TextControl)group.GetChildByName("SpotifyClientSecret")).Value;
                var redirectUri = ((TextControl)group.GetChildByName("SpotifyRedirectUri")).Value;

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
                var clientId = ((TextControl)group.GetChildByName("SpotifyClientId")).Value;
                var clientSecret = ((TextControl)group.GetChildByName("SpotifyClientSecret")).Value;
                var redirectUri = ((TextControl)group.GetChildByName("SpotifyRedirectUri")).Value;

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
                    new BoolControlFactory()
                    {
                        LabelText = Strings.Enable,
                        Converter = new BoolConverter(parser, sectionName, "Enabled"),
                    },
                ]
                : [];

            Controls = [
                ..enabledControls,
                new ButtonControlFactory()
                {
                    LabelText = "",
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
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.RectLeft,
                    Name = "WindowLeft",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowLeft"),
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.RectTop,
                    Name = "WindowTop",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowTop"),
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.RectWidth,
                    Name = "WindowWidth",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowWidth"),
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.RectHeight,
                    Name = "WindowHeight",
                    Converter = new IntNumberConverter(parser, sectionName, "WindowHeight"),
                },
                new NumberControlFactory()
                {
                    LabelText = Strings.Scale,
                    Converter = new FloatNumberConverter(parser, sectionName, "ContentScale"),
                },
                new SelectControlFactory()
                {
                    LabelText = Strings.Rotation,
                    Items = new()
                    {
                        new("-90 °", -90),
                        new("0 °", 0),
                        new("90 °", 90),
                        new("180 °", 180),
                    },
                    Converter = new IntSelectConverter(parser, sectionName, "ContentRotation"),
                },
            ];
        }
    }
}
