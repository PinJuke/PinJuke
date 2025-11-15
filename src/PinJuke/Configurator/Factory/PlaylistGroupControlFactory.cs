using PinJuke.Configuration;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PinJuke.Configurator.Factory
{
    public class PlaylistGroupControlFactory : GroupControlFactory
    {
        public PlaylistGroupControlFactory(Parser parser, MediaPathProvider mediaPathProvider)
        {
            LabelText = Strings.PlaylistConfiguration;
            Controls = [
                new GroupControlFactory()
                {
                    LabelText = Strings.Player,
                    Controls = [
                        new SelectControlFactory()
                        {
                            Name = "PlayerSourceType",
                            LabelText = "Music Source",
                            Items = new()
                            {
                                new("Local MP3 Files", 0),
                                new("Spotify Playlist", 1),
                            },
                            Converter = new IntSelectConverter(parser, "Player", "SourceType"),
                            ChangedHandler = async (ConfiguratorControl control) =>
                            {
                                var value = ((SelectControl)control).SelectedValue;
                                var isLocalFiles = value is int intValue && intValue == 0;
                                var group = control.GetParentGroup();
                                ((PathControl)group.GetChildByName("PlayerMusicPath")).IsEnabled = isLocalFiles;
                                ((SelectControl)group.GetChildByName("PlayerSpotifyPlaylist")).IsEnabled = !isLocalFiles;
                                ((ButtonControl)group.GetChildByName("RefreshSpotifyPlaylistsButton")).IsEnabled = !isLocalFiles;
                                
                                // If Spotify is selected, load playlists
                                if (!isLocalFiles)
                                {
                                    var playlistControl = (SelectControl)group.GetChildByName("PlayerSpotifyPlaylist");
                                    await LoadSpotifyPlaylistsAsync(playlistControl, parser);
                                }
                            },
                        },
                        new PathControlFactory()
                        {
                            Name = "PlayerMusicPath",
                            LabelText = Strings.MusicPath,
                            FileMode = false,
                            RelativeEnabled = false,
                            Converter = new PathConverter(parser, "Player", "MusicPath"),
                        },
                        new SelectControlFactory()
                        {
                            Name = "PlayerSpotifyPlaylist",
                            LabelText = "Spotify Playlist",
                            Items = new()
                            {
                                new("(Loading playlists...)", "")
                            },
                            Converter = new StringSelectConverter(parser, "Player", "SpotifyPlaylistId"),
                        },
                        new ButtonControlFactory()
                        {
                            Name = "RefreshSpotifyPlaylistsButton",
                            Text = "Refresh Playlists",
                            ClickHandler = async (control) =>
                            {
                                try
                                {
                                    System.Diagnostics.Debug.WriteLine("=== REFRESH BUTTON CLICKED ===");
                                    
                                    var group = control.GetParentGroup();
                                    var playlistControl = (SelectControl)group.GetChildByName("PlayerSpotifyPlaylist");
                                    
                                    // Store current selection to preserve it if possible
                                    var currentSelection = playlistControl.SelectedValue;
                                    
                                    // Show loading state
                                    playlistControl.Items = new List<Item> { new Item("Refreshing playlists...", "") };
                                    
                                    await LoadSpotifyPlaylistsAsync(playlistControl, parser);
                                    
                                    // Try to restore selection if the playlist still exists
                                    if (currentSelection != null)
                                    {
                                        var matchingItem = playlistControl.Items.FirstOrDefault(item => 
                                            item.Value != null && item.Value.Equals(currentSelection));
                                        if (matchingItem != null)
                                        {
                                            playlistControl.SelectedValue = currentSelection;
                                        }
                                    }
                                    
                                    System.Windows.MessageBox.Show("Playlist refresh completed!", "PinJuke", System.Windows.MessageBoxButton.OK);
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.MessageBox.Show($"Error refreshing playlists: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK);
                                }
                            },
                        },
                        new BoolControlFactory()
                        {
                            Name = "PlayerShufflePlaylist",
                            LabelText = "Shuffle Playlist (randomize song order)",
                            Converter = new BoolConverter(parser, "Player", "ShufflePlaylist"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.StartupTrackType,
                            Items = new()
                            {
                                new(Strings.StartupTrackTypeLastPlayedTrack, 0),
                                new(Strings.StartupTrackTypeFirstTrack, 1),
                                new(Strings.StartupTrackTypeRandomMode, 2),
                            },
                            Converter = new IntSelectConverter(parser, "Player", "StartupTrackType"),
                        },
                        new BoolControlFactory()
                        {
                            LabelText = Strings.PlayOnStartup,
                            Converter = new BoolConverter(parser, "Player", "PlayOnStartup"),
                        },
                        new SelectControlFactory()
                        {
                            LabelText = Strings.TrackBrowserOn,
                            Items = new()
                            {
                                new(Strings.DisplayPlayField, 0),
                                new(Strings.DisplayBackGlass, 1),
                                new(Strings.DisplayDmd, 2),
                            },
                            Converter = new TrackBrowserOnConverter(parser),
                        },
                    ]
                },
                new ContentGroupControlFactory(parser, "PlayField", mediaPathProvider)
                {
                    LabelText = Strings.DisplayPlayField,
                },
                new ContentGroupControlFactory(parser, "BackGlass", mediaPathProvider)
                {
                    LabelText = Strings.DisplayBackGlass,
                },
                new ContentGroupControlFactory(parser, "DMD", mediaPathProvider)
                {
                    LabelText = Strings.DisplayDmd,
                },
            ];
        }

        private static async Task LoadSpotifyPlaylistsAsync(SelectControl playlistControl, Parser parser)
        {
            try
            {
                // Read Spotify configuration from the global config file
                var globalConfigPath = PinJuke.Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var iniDoc = PinJuke.Ini.IniReader.TryRead(globalConfigPath) ?? new PinJuke.Ini.IniDocument();
                
                var spotifySection = iniDoc["Spotify"];
                
                if (spotifySection == null)
                {
                    playlistControl.Items = new List<Item> { new Item("(Spotify not configured in Global Config)", "") };
                    return;
                }

                var clientId = parser.ParseString(spotifySection["ClientId"]);
                var clientSecret = parser.ParseString(spotifySection["ClientSecret"]);
                var redirectUri = parser.ParseString(spotifySection["RedirectUri"]) ?? "http://127.0.0.1:8888/callback";
                var enabled = parser.ParseBool(spotifySection["Enabled"]) ?? false;

                if (!enabled || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    playlistControl.Items = new List<Item> { new Item("(Authenticate in Global Config first)", "") };
                    return;
                }

                // Check for saved authentication tokens
                var accessToken = parser.ParseString(spotifySection["AccessToken"]);
                var refreshToken = parser.ParseString(spotifySection["RefreshToken"]);
                var expiresAtStr = parser.ParseString(spotifySection["ExpiresAt"]);
                var scopesStr = parser.ParseString(spotifySection["Scopes"]);

                if (string.IsNullOrEmpty(accessToken))
                {
                    playlistControl.Items = new List<Item> { new Item("(Authenticate in Global Config first)", "") };
                    return;
                }

                // Check if token is expired
                DateTime expiresAt = DateTime.Now.AddDays(-1); // Default to expired
                if (!string.IsNullOrEmpty(expiresAtStr) && DateTime.TryParse(expiresAtStr, out var parsedDate))
                {
                    expiresAt = parsedDate;
                }

                // Create temporary Spotify service with saved tokens
                var spotifyConfig = new PinJuke.Spotify.SpotifyConfig
                {
                    Enabled = true,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    RedirectUri = redirectUri
                };

                using var spotifyService = new PinJuke.Spotify.SpotifyService();
                await spotifyService.InitializeAsync(spotifyConfig);

                // Create auth result from saved tokens
                var authResult = new PinJuke.Spotify.SpotifyAuthResult
                {
                    IsSuccess = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken ?? "",
                    ExpiresAt = expiresAt,
                    Scopes = !string.IsNullOrEmpty(scopesStr) ? scopesStr.Split(',') : Array.Empty<string>()
                };

                // Set the authentication in the service
                spotifyService.AuthService.SetAuthResult(authResult);

                // Check if token is still valid, refresh if needed
                if (authResult.IsExpired && !string.IsNullOrEmpty(authResult.RefreshToken))
                {
                    System.Diagnostics.Debug.WriteLine("Token expired, attempting refresh...");
                    var refreshResult = await spotifyService.AuthService.RefreshTokenAsync();
                    if (!refreshResult.IsSuccess)
                    {
                        playlistControl.Items = new List<Item> { new Item("(Authentication expired - re-authenticate in Global Config)", "") };
                        return;
                    }
                    
                    // Save the refreshed tokens
                    System.Diagnostics.Debug.WriteLine("Token refreshed successfully, saving new tokens...");
                    SaveUpdatedAuthTokens(refreshResult);
                }
                else if (authResult.IsExpired)
                {
                    playlistControl.Items = new List<Item> { new Item("(Authentication expired - re-authenticate in Global Config)", "") };
                    return;
                }

                // Load playlists
                var newItems = new List<Item>();
                newItems.Add(new Item("Loading playlists...", ""));
                playlistControl.Items = newItems;

                System.Diagnostics.Debug.WriteLine("Fetching playlists from Spotify API...");
                var playlists = await spotifyService.GetUserPlaylistsAsync();
                System.Diagnostics.Debug.WriteLine($"Retrieved {playlists.Count} playlists from Spotify");
                
                // Create a fresh list with the new playlists
                var finalItems = new List<Item>();
                if (playlists.Count == 0)
                {
                    finalItems.Add(new Item("(No playlists found)", ""));
                }
                else
                {
                    foreach (var playlist in playlists)
                    {
                        System.Diagnostics.Debug.WriteLine($"Adding playlist: {playlist.Name} (ID: {playlist.Id})");
                        finalItems.Add(new Item($"{playlist.Name} ({playlist.TrackCount} tracks)", playlist.Id));
                    }
                }
                
                // Assign the new list to trigger property change notification
                playlistControl.Items = finalItems;
            }
            catch (Exception ex)
            {
                var errorItems = new List<Item> { new Item($"(Error loading playlists: {ex.Message})", "") };
                playlistControl.Items = errorItems;
            }
        }

        private static void SaveUpdatedAuthTokens(PinJuke.Spotify.SpotifyAuthResult authResult)
        {
            try
            {
                var globalConfigPath = PinJuke.Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var iniDoc = PinJuke.Ini.IniReader.TryRead(globalConfigPath) ?? new PinJuke.Ini.IniDocument();
                
                var spotifySection = iniDoc["Spotify"];
                spotifySection["AccessToken"] = authResult.AccessToken;
                spotifySection["RefreshToken"] = authResult.RefreshToken;
                spotifySection["ExpiresAt"] = authResult.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss");
                spotifySection["Scopes"] = string.Join(",", authResult.Scopes);
                
                // Save the updated configuration
                using var textWriter = new System.IO.StreamWriter(globalConfigPath);
                iniDoc.WriteTo(textWriter);
                
                System.Diagnostics.Debug.WriteLine("Updated Spotify auth tokens saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving updated auth tokens: {ex.Message}");
            }
        }
    }

    public class TrackBrowserOnConverter : Converter<SelectControl>
    {
        public Parser Parser { get; }

        public TrackBrowserOnConverter(Parser parser)
        {
            Parser = parser;
        }

        public void ReadFromControl(SelectControl control, IniDocument iniDocument)
        {
            var selectedValue = (int?)control.SelectedValue;
            iniDocument["PlayField"]["BrowserEnabled"] = Parser.FormatBool(selectedValue == 0);
            iniDocument["BackGlass"]["BrowserEnabled"] = Parser.FormatBool(selectedValue == 1);
            iniDocument["DMD"]["BrowserEnabled"] = Parser.FormatBool(selectedValue == 2);
        }

        public void WriteToControl(SelectControl control, IniDocument iniDocument)
        {
            int? selectedValue = null;
            if (Parser.ParseBool(iniDocument["PlayField"]["BrowserEnabled"]) == true)
            {
                selectedValue = 0;
            }
            else if (Parser.ParseBool(iniDocument["BackGlass"]["BrowserEnabled"]) == true)
            {
                selectedValue = 1;
            }
            else if (Parser.ParseBool(iniDocument["DMD"]["BrowserEnabled"]) == true)
            {
                selectedValue = 2;
            }
            control.SelectedValue = selectedValue;
        }
    }

    public class ContentGroupControlFactory : GroupControlFactory
    {
        public const string BACKGROUND_IMAGE_FILE_CONTROL = "BackgroundImageFile";
        public const string THEME_VIDEO_START_FILE_CONTROL = "ThemeVideoStartFile";
        public const string THEME_VIDEO_LOOP_FILE_CONTROL = "ThemeVideoLoopFile";
        public const string THEME_VIDEO_STOP_FILE_CONTROL = "ThemeVideoStopFile";
        public const string THEME_VIDEO_ROTATION_CONTROL = "ThemeVideoRotation";

        public ContentGroupControlFactory(Parser parser, string sectionName, MediaPathProvider mediaPathProvider)
        {
            Controls = [
                new SelectControlFactory()
                {
                    LabelText = Strings.BackgroundType,
                    Items = new()
                    {
                        new(Strings.BackgroundTypeShowSpecifiedImage, 0),
                        new(Strings.BackgroundTypeShowMilkdropVisualizations, 1),
                    },
                    Converter = new IntSelectConverter(parser, sectionName, "BackgroundType"),
                    ChangedHandler = (ConfiguratorControl control) =>
                    {
                        var value = ((SelectControl)control).SelectedValue;
                        var enabled = value is int intValue && intValue == 0;
                        var group = control.GetParentGroup();
                        ((PathControl)group.GetChildByName(BACKGROUND_IMAGE_FILE_CONTROL)).Enabled = enabled;
                    },
                },
                new PathControlFactory()
                {
                    Name = BACKGROUND_IMAGE_FILE_CONTROL,
                    LabelText = Strings.BackgroundImageFile,
                    FileMode = true,
                    RelativeEnabled = true,
                    FileExtension = ".jpg",
                    FileFilter = $"{Strings.JpegFile}|*.jpg;*.jpeg;*.jpe;*.jfif",
                    MediaPathProvider = mediaPathProvider,
                    Converter = new PathConverter(parser, sectionName, "BackgroundImageFile"),
                },
                new BoolControlFactory()
                {
                    LabelText = Strings.EnableTrackCover,
                    Converter = new BoolConverter(parser, sectionName, "CoverEnabled"),
                },
                new BoolControlFactory()
                {
                    LabelText = Strings.EnablePlaybackStatus,
                    Converter = new BoolConverter(parser, sectionName, "StateEnabled"),
                },
                new BoolControlFactory()
                {
                    LabelText = Strings.EnableThemeVideo,
                    Converter = new BoolConverter(parser, sectionName, "ThemeVideoEnabled"),
                    ChangedHandler = (ConfiguratorControl control) =>
                    {
                        var enabled = ((BoolControl)control).Value;
                        var group = control.GetParentGroup();
                        ((PathControl)group.GetChildByName(THEME_VIDEO_START_FILE_CONTROL)).Enabled = enabled;
                        ((PathControl)group.GetChildByName(THEME_VIDEO_LOOP_FILE_CONTROL)).Enabled = enabled;
                        ((PathControl)group.GetChildByName(THEME_VIDEO_STOP_FILE_CONTROL)).Enabled = enabled;
                        ((SelectControl)group.GetChildByName(THEME_VIDEO_ROTATION_CONTROL)).Enabled = enabled;
                    },
                },
                new PathControlFactory()
                {
                    Name = THEME_VIDEO_START_FILE_CONTROL,
                    LabelText = Strings.ThemeVideoStartFile,
                    FileMode = true,
                    RelativeEnabled = true,
                    FileExtension = ".mp4",
                    FileFilter = $"{Strings.Mp4File}|*.mp4",
                    MediaPathProvider = mediaPathProvider,
                    Converter = new PathConverter(parser, sectionName, "ThemeVideoStartFile"),
                },
                new PathControlFactory()
                {
                    Name = THEME_VIDEO_LOOP_FILE_CONTROL,
                    LabelText = Strings.ThemeVideoLoopFile,
                    FileMode = true,
                    RelativeEnabled = true,
                    FileExtension = ".mp4",
                    FileFilter = $"{Strings.Mp4File}|*.mp4",
                    MediaPathProvider = mediaPathProvider,
                    Converter = new PathConverter(parser, sectionName, "ThemeVideoLoopFile"),
                },
                new PathControlFactory()
                {
                    Name = THEME_VIDEO_STOP_FILE_CONTROL,
                    LabelText = Strings.ThemeVideoStopFile,
                    FileMode = true,
                    RelativeEnabled = true,
                    FileExtension = ".mp4",
                    FileFilter = $"{Strings.Mp4File}|*.mp4",
                    MediaPathProvider = mediaPathProvider,
                    Converter = new PathConverter(parser, sectionName, "ThemeVideoStopFile"),
                },
                new SelectControlFactory()
                {
                    Name = THEME_VIDEO_ROTATION_CONTROL,
                    LabelText = Strings.ThemeVideoRotation,
                    Items = new()
                    {
                        new("-90 °", -90),
                        new("0 °", 0),
                        new("90 °", 90),
                        new("180 °", 180),
                    },
                    Converter = new IntSelectConverter(parser, sectionName, "ThemeVideoRotation"),
                },
            ];
        }
    }
}
