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
                        new RowFactory<SelectControl>() {
                            LabelText = "Music Source",
                            ChildFactory = new SelectControlFactory()
                            {
                                Name = "PlayerSourceType",
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
                                    
                                    // Safely update controls if they exist
                                    try
                                    {
                                        var musicPathControl = group.GetChildByName("PlayerMusicPath") as PathControl;
                                        if (musicPathControl != null) musicPathControl.IsEnabled = isLocalFiles;
                                        
                                        var spotifyPlaylistControl = group.GetChildByName("PlayerSpotifyPlaylist") as SelectControl;
                                        if (spotifyPlaylistControl != null) spotifyPlaylistControl.IsEnabled = !isLocalFiles;
                                        
                                        var refreshButtonControl = group.GetChildByName("RefreshSpotifyPlaylistsButton") as ButtonControl;
                                        if (refreshButtonControl != null) refreshButtonControl.IsEnabled = !isLocalFiles;
                                        
                                        // If Spotify is selected, load playlists
                                        if (!isLocalFiles && spotifyPlaylistControl != null)
                                        {
                                            await LoadSpotifyPlaylistsAsync(spotifyPlaylistControl, parser);
                                        }
                                    }
                                    catch
                                    {
                                        // Controls may not exist yet, ignore
                                    }
                                },
                            }
                        },
                        new RowFactory<PathControl>() {
                            LabelText = Strings.MusicPath,
                            ChildFactory = new PathControlFactory()
                            {
                                Name = "PlayerMusicPath",
                                FileMode = false,
                                RelativeEnabled = false,
                                Converter = new PathConverter(parser, "Player", "MusicPath"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = "Spotify Playlist",
                            ChildFactory = new SelectControlFactory()
                            {
                                Name = "PlayerSpotifyPlaylist",
                                Items = new()
                                {
                                    new("(Loading playlists...)", "")
                                },
                                Converter = new StringSelectConverter(parser, "Player", "SpotifyPlaylistId"),
                            }
                        },
                        new RowFactory<ButtonControl>() {
                            LabelText = "Playlist Management",
                            ChildFactory = new ButtonControlFactory()
                            {
                                Name = "RefreshSpotifyPlaylistsButton",
                                Text = "Refresh Playlists",
                                ClickHandler = async (control) =>
                                {
                                    try
                                    {
                                        System.Diagnostics.Debug.WriteLine("=== REFRESH BUTTON CLICKED ===");
                                        
                                        var group = control.GetParentGroup();
                                        var playlistControl = group.GetChildByName("PlayerSpotifyPlaylist") as SelectControl;
                                        
                                        if (playlistControl == null)
                                        {
                                            System.Windows.MessageBox.Show("Error refreshing playlist: Cannot return child. Found no control for \"PlayerSpotifyPlaylist\".", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                                            return;
                                        }
                                        
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
                            }
                        },
                        new RowFactory<BoolControl>() {
                            LabelText = "Shuffle Playlist (randomize song order)",
                            ChildFactory = new BoolControlFactory()
                            {
                                Name = "PlayerShufflePlaylist",
                                Converter = new BoolConverter(parser, "Player", "ShufflePlaylist"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.StartupTrackType,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = new()
                                {
                                    new(Strings.StartupTrackTypeLastPlayedTrack, 0),
                                    new(Strings.StartupTrackTypeFirstTrack, 1),
                                    new(Strings.StartupTrackTypeRandomMode, 2),
                                },
                                Converter = new IntSelectConverter(parser, "Player", "StartupTrackType"),
                            }
                        },
                        new RowFactory<BoolControl>() {
                            LabelText = Strings.PlayOnStartup,
                            ChildFactory = new BoolControlFactory()
                            {
                                Converter = new BoolConverter(parser, "Player", "PlayOnStartup"),
                            }
                        },
                        new RowFactory<SelectControl>() {
                            LabelText = Strings.TrackBrowserOn,
                            ChildFactory = new SelectControlFactory()
                            {
                                Items = new()
                                {
                                    new(Strings.DisplayPlayField, 0),
                                    new(Strings.DisplayBackGlass, 1),
                                    new(Strings.DisplayDmd, 2),
                                },
                                Converter = new TrackBrowserOnConverter(parser),
                            }
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
                new RowFactory<SelectControl>()
                {
                    LabelText = Strings.PlaybackBackgroundType,
                    ChildFactory = new SelectControlFactory()
                    {
                        Items = new()
                        {
                            new(Strings.BackgroundTypeShowSpecifiedImage, 0),
                            new(Strings.BackgroundTypeShowMilkdropVisualizations, 1),
                            new(Strings.BackgroundTypeShowLoopVideo, 2),
                        },
                        Converter = new IntSelectConverter(parser, sectionName, "PlaybackBackgroundType"),
                        ChangedHandler = (ConfiguratorControl control) =>
                        {
                            var value = ((SelectControl)control).SelectedValue;
                            var group = control.GetParentGroup();
                            // Enable image file control only for Image background type (0)
                            var isImageType = value is int intValue && intValue == 0;
                            var backgroundImageControl = group.GetChildByName(BACKGROUND_IMAGE_FILE_CONTROL) as PathControl;
                            if (backgroundImageControl != null) backgroundImageControl.Enabled = isImageType;
                            // Video-related controls would be handled elsewhere since this is the background type selector
                        },
                    }
                },
                new RowFactory<PathControl>()
                {
                    LabelText = Strings.BackgroundImageFile,
                    ChildFactory = new PathControlFactory()
                    {
                        Name = BACKGROUND_IMAGE_FILE_CONTROL,
                        FileMode = true,
                        RelativeEnabled = true,
                        FileExtension = ".jpg",
                        FileFilter = $"{Strings.JpegFile}|*.jpg;*.jpeg;*.jpe;*.jfif",
                        MediaPathProvider = mediaPathProvider,
                        Converter = new PathConverter(parser, sectionName, "BackgroundImageFile"),
                    }
                },
                new RowFactory<BoolControl>()
                {
                    LabelText = Strings.EnableTrackCover,
                    ChildFactory = new BoolControlFactory()
                    {
                        Converter = new BoolConverter(parser, sectionName, "CoverEnabled"),
                    }
                },
                new RowFactory<BoolControl>()
                {
                    LabelText = Strings.EnableThemeVideo,
                    ChildFactory = new BoolControlFactory()
                    {
                        Converter = new BoolConverter(parser, sectionName, "ThemeVideoEnabled"),
                        ChangedHandler = (ConfiguratorControl control) =>
                        {
                            var enabled = ((BoolControl)control).Value;
                            var group = control.GetParentGroup();
                            
                            // Safely update video controls if they exist
                            try
                            {
                                var startFileControl = group.GetChildByName(THEME_VIDEO_START_FILE_CONTROL) as PathControl;
                                if (startFileControl != null) startFileControl.Enabled = enabled;
                                
                                var loopFileControl = group.GetChildByName(THEME_VIDEO_LOOP_FILE_CONTROL) as PathControl;
                                if (loopFileControl != null) loopFileControl.Enabled = enabled;
                                
                                var stopFileControl = group.GetChildByName(THEME_VIDEO_STOP_FILE_CONTROL) as PathControl;
                                if (stopFileControl != null) stopFileControl.Enabled = enabled;
                                
                                var rotationControl = group.GetChildByName(THEME_VIDEO_ROTATION_CONTROL) as SelectControl;
                                if (rotationControl != null) rotationControl.Enabled = enabled;
                            }
                            catch
                            {
                                // Controls may not exist yet, ignore
                            }
                        },
                    }
                },
                new RowFactory<PathControl>()
                {
                    LabelText = Strings.ThemeVideoStartFile,
                    ChildFactory = new PathControlFactory()
                    {
                        Name = THEME_VIDEO_START_FILE_CONTROL,
                        FileMode = true,
                        RelativeEnabled = true,
                        FileExtension = ".mp4",
                        FileFilter = $"{Strings.Mp4File}|*.mp4",
                        MediaPathProvider = mediaPathProvider,
                        Converter = new PathConverter(parser, sectionName, "ThemeVideoStartFile"),
                    }
                },
                new RowFactory<PathControl>()
                {
                    LabelText = Strings.ThemeVideoLoopFile,
                    ChildFactory = new PathControlFactory()
                    {
                        Name = THEME_VIDEO_LOOP_FILE_CONTROL,
                        FileMode = true,
                        RelativeEnabled = true,
                        FileExtension = ".mp4",
                        FileFilter = $"{Strings.Mp4File}|*.mp4",
                        MediaPathProvider = mediaPathProvider,
                        Converter = new PathConverter(parser, sectionName, "ThemeVideoLoopFile"),
                    }
                },
                new RowFactory<PathControl>()
                {
                    LabelText = Strings.ThemeVideoStopFile,
                    ChildFactory = new PathControlFactory()
                    {
                        Name = THEME_VIDEO_STOP_FILE_CONTROL,
                        FileMode = true,
                        RelativeEnabled = true,
                        FileExtension = ".mp4",
                        FileFilter = $"{Strings.Mp4File}|*.mp4",
                        MediaPathProvider = mediaPathProvider,
                        Converter = new PathConverter(parser, sectionName, "ThemeVideoStopFile"),
                    }
                },
                new RowFactory<SelectControl>()
                {
                    LabelText = Strings.ThemeVideoRotation,
                    ChildFactory = new SelectControlFactory()
                    {
                        Name = THEME_VIDEO_ROTATION_CONTROL,
                        Items = new()
                        {
                            new("-90 °", -90),
                            new("0 °", 0),
                            new("90 °", 90),
                            new("180 °", 180),
                        },
                        Converter = new IntSelectConverter(parser, sectionName, "ThemeVideoRotation"),
                    }
                },
            ];
        }
    }
}
