using PinJuke.Configuration;
using PinJuke.Playlist;
using PinJuke.Spotify;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PinJuke.Controller
{
    /// <summary>
    /// Scanner for Spotify playlists - creates FileNode structure from Spotify tracks
    /// </summary>
    public class SpotifyScanner : BackgroundWorker
    {
        private readonly Player playerConfig;
        private readonly SpotifyConfig spotifyConfig;

        public SpotifyScanner(Player playerConfig, SpotifyConfig spotifyConfig)
        {
            WorkerSupportsCancellation = true;
            this.playerConfig = playerConfig;
            this.spotifyConfig = spotifyConfig;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            try
            {
                Debug.WriteLine("SpotifyScanner: Starting Spotify playlist scan...");
                
                if (CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                var scanResult = Task.Run(async () => await ScanSpotifyPlaylistAsync()).Result;
                e.Result = scanResult;
                
                Debug.WriteLine("SpotifyScanner: Spotify playlist scan completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyScanner: Error during Spotify scan: {ex.Message}");
                
                // Create an empty scan result with an error node
                var errorRoot = new FileNode("", "Spotify Error", FileType.Directory);
                errorRoot.AppendChild(new FileNode("error", $"Failed to load Spotify playlist: {ex.Message}", FileType.Music));
                e.Result = new ScanResult(errorRoot);
            }
        }

        private async Task<ScanResult> ScanSpotifyPlaylistAsync()
        {
            if (CancellationPending)
            {
                throw new OperationCanceledException();
            }

            Debug.WriteLine($"SpotifyScanner: Loading playlist {playerConfig.SpotifyPlaylistId}");

            // Create Spotify service
            using var spotifyService = new SpotifyService();
            await spotifyService.InitializeAsync(spotifyConfig);

            // Load authentication from global config
            var authResult = LoadSpotifyAuthentication();
            if (authResult == null)
            {
                throw new InvalidOperationException("Spotify authentication not found. Please authenticate in Global Config.");
            }

            spotifyService.AuthService.SetAuthResult(authResult);

            // Check if token is expired and refresh if needed
            if (authResult.IsExpired && !string.IsNullOrEmpty(authResult.RefreshToken))
            {
                Debug.WriteLine("SpotifyScanner: Token expired, refreshing...");
                var refreshResult = await spotifyService.AuthService.RefreshTokenAsync();
                if (!refreshResult.IsSuccess)
                {
                    throw new InvalidOperationException("Spotify authentication expired and could not be refreshed. Please re-authenticate in Global Config.");
                }
                // Update the auth result with refreshed tokens
                authResult = refreshResult;
                spotifyService.AuthService.SetAuthResult(authResult);
            }
            else if (authResult.IsExpired)
            {
                throw new InvalidOperationException("Spotify authentication expired. Please re-authenticate in Global Config.");
            }

            if (CancellationPending)
            {
                throw new OperationCanceledException();
            }

            // Get the playlist
            var playlist = await spotifyService.GetPlaylistAsync(playerConfig.SpotifyPlaylistId);
            if (playlist == null)
            {
                throw new InvalidOperationException($"Spotify playlist '{playerConfig.SpotifyPlaylistId}' not found or could not be loaded.");
            }

            Debug.WriteLine($"SpotifyScanner: Loaded playlist '{playlist.Name}' with {playlist.Tracks.Count} tracks");

            // Create root node for the playlist
            var rootNode = new FileNode("", playlist.Name, FileType.Directory);

            // Get tracks and shuffle if enabled
            var tracks = playlist.Tracks;
            if (playerConfig.ShufflePlaylist)
            {
                Debug.WriteLine("SpotifyScanner: Shuffling playlist tracks");
                var random = new Random();
                tracks = tracks.OrderBy(x => random.Next()).ToList();
            }

            // Add tracks as child nodes
            foreach (var track in tracks)
            {
                if (CancellationPending)
                {
                    throw new OperationCanceledException();
                }

                // Create a unique identifier for the track (using Spotify URI format)
                var trackUri = $"spotify:track:{track.Id}";
                var displayName = $"{track.Name} - {string.Join(", ", track.Artists)}";
                
                var trackNode = new FileNode(trackUri, displayName, FileType.SpotifyTrack);
                rootNode.AppendChild(trackNode);
            }

            Debug.WriteLine("SpotifyScanner: Created track nodes for playlist");
            return new ScanResult(rootNode);
        }

        private SpotifyAuthResult? LoadSpotifyAuthentication()
        {
            try
            {
                var globalConfigPath = PinJuke.Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
                var iniDoc = PinJuke.Ini.IniReader.TryRead(globalConfigPath);
                if (iniDoc == null) return null;

                var spotifySection = iniDoc["Spotify"];
                if (spotifySection == null) return null;

                var parser = new PinJuke.Configuration.Parser();
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

                return new SpotifyAuthResult
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
                Debug.WriteLine($"SpotifyScanner: Error loading authentication: {ex.Message}");
                return null;
            }
        }
    }
}
