using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Service for controlling Spotify playback (requires Spotify app to be running)
    /// </summary>
    public class SpotifyPlaybackController : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly SpotifyConfig config;
        private SpotifyAuthResult? authResult;

        public SpotifyPlaybackController(SpotifyConfig config)
        {
            this.config = config;
            this.httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PinJuke/1.0");
        }

        public void SetAuthResult(SpotifyAuthResult authResult)
        {
            this.authResult = authResult;
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        }

        /// <summary>
        /// Get available devices
        /// </summary>
        public async Task<List<SpotifyDevice>> GetAvailableDevicesAsync()
        {
            try
            {
                var response = await httpClient.GetAsync("https://api.spotify.com/v1/me/player/devices");
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Failed to get devices: {response.StatusCode}");
                    return new List<SpotifyDevice>();
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var devices = new List<SpotifyDevice>();

                if (jsonDoc.RootElement.TryGetProperty("devices", out var devicesArray))
                {
                    foreach (var deviceJson in devicesArray.EnumerateArray())
                    {
                        var device = new SpotifyDevice
                        {
                            Id = deviceJson.GetProperty("id").GetString() ?? "",
                            Name = deviceJson.GetProperty("name").GetString() ?? "",
                            Type = deviceJson.GetProperty("type").GetString() ?? "",
                            IsActive = deviceJson.GetProperty("is_active").GetBoolean(),
                            IsRestricted = deviceJson.GetProperty("is_restricted").GetBoolean(),
                            IsPrivateSession = deviceJson.GetProperty("is_private_session").GetBoolean(),
                            VolumePercent = deviceJson.GetProperty("volume_percent").GetInt32(),
                            SupportsVolume = deviceJson.GetProperty("supports_volume").GetBoolean()
                        };
                        devices.Add(device);
                    }
                }

                return devices;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting devices: {ex.Message}");
                return new List<SpotifyDevice>();
            }
        }

        /// <summary>
        /// Transfer playback to specified device
        /// </summary>
        public async Task<bool> TransferPlaybackAsync(string deviceId, bool play = false)
        {
            try
            {
                var transferData = new
                {
                    device_ids = new[] { deviceId },
                    play = play
                };

                var json = JsonSerializer.Serialize(transferData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync("https://api.spotify.com/v1/me/player", content);
                
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Successfully transferred playback to device {deviceId}");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Failed to transfer playback: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error transferring playback: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Start playback of specific track
        /// </summary>
        public async Task<bool> PlayTrackAsync(string trackUri, string? deviceId = null)
        {
            try
            {
                Debug.WriteLine($"SpotifyPlaybackController: Attempting to play track {trackUri}");
                Debug.WriteLine($"SpotifyPlaybackController: Target device ID: {deviceId ?? "default"}");
                Debug.WriteLine($"SpotifyPlaybackController: Has auth token: {authResult?.AccessToken != null}");
                
                if (authResult == null)
                {
                    Debug.WriteLine("SpotifyPlaybackController: ERROR - No authentication set");
                    return false;
                }
                
                if (authResult.IsExpired)
                {
                    Debug.WriteLine("SpotifyPlaybackController: ERROR - Authentication token expired");
                    return false;
                }

                var playData = new
                {
                    uris = new[] { trackUri }
                };

                var json = JsonSerializer.Serialize(playData);
                Debug.WriteLine($"SpotifyPlaybackController: Request body: {json}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = "https://api.spotify.com/v1/me/player/play";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    url += $"?device_id={deviceId}";
                }
                
                Debug.WriteLine($"SpotifyPlaybackController: Request URL: {url}");

                var response = await httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"SpotifyPlaybackController: ✅ Successfully started playback of {trackUri}");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"SpotifyPlaybackController: ❌ Failed to start playback!");
                    Debug.WriteLine($"  Status Code: {response.StatusCode} ({(int)response.StatusCode})");
                    Debug.WriteLine($"  Response: {responseContent}");
                    
                    // Provide specific guidance based on error code
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.NotFound:
                            Debug.WriteLine("  → Check: Device ID might be invalid or device not active");
                            break;
                        case System.Net.HttpStatusCode.Forbidden:
                            Debug.WriteLine("  → Check: User needs Spotify Premium subscription");
                            break;
                        case System.Net.HttpStatusCode.Unauthorized:
                            Debug.WriteLine("  → Check: Authentication token might be invalid");
                            break;
                        case System.Net.HttpStatusCode.BadRequest:
                            Debug.WriteLine("  → Check: Request format or track URI might be invalid");
                            break;
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SpotifyPlaybackController: Exception during playback: {ex.Message}");
                Debug.WriteLine($"SpotifyPlaybackController: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Start playback of playlist
        /// </summary>
        public async Task<bool> PlayPlaylistAsync(string playlistUri, string? deviceId = null, int offset = 0)
        {
            try
            {
                var playData = new
                {
                    context_uri = playlistUri,
                    offset = new { position = offset }
                };

                var json = JsonSerializer.Serialize(playData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = "https://api.spotify.com/v1/me/player/play";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    url += $"?device_id={deviceId}";
                }

                var response = await httpClient.PutAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Successfully started playlist playback");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Failed to start playlist playback: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting playlist playback: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Pause playback
        /// </summary>
        public async Task<bool> PauseAsync(string? deviceId = null)
        {
            try
            {
                var url = "https://api.spotify.com/v1/me/player/pause";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    url += $"?device_id={deviceId}";
                }

                var response = await httpClient.PutAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error pausing playback: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resume playback
        /// </summary>
        public async Task<bool> ResumeAsync(string? deviceId = null)
        {
            try
            {
                var url = "https://api.spotify.com/v1/me/player/play";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    url += $"?device_id={deviceId}";
                }

                var response = await httpClient.PutAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resuming playback: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Skip to next track
        /// </summary>
        public async Task<bool> NextTrackAsync(string? deviceId = null)
        {
            try
            {
                var url = "https://api.spotify.com/v1/me/player/next";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    url += $"?device_id={deviceId}";
                }

                var response = await httpClient.PostAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error skipping to next track: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Skip to previous track
        /// </summary>
        public async Task<bool> PreviousTrackAsync(string? deviceId = null)
        {
            try
            {
                var url = "https://api.spotify.com/v1/me/player/previous";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    url += $"?device_id={deviceId}";
                }

                var response = await httpClient.PostAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error skipping to previous track: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set volume
        /// </summary>
        public async Task<bool> SetVolumeAsync(int volumePercent, string? deviceId = null)
        {
            try
            {
                var url = $"https://api.spotify.com/v1/me/player/volume?volume_percent={volumePercent}";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    url += $"&device_id={deviceId}";
                }

                var response = await httpClient.PutAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting volume: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the current playback state
        /// </summary>
        public async Task<SpotifyCurrentlyPlaying?> GetCurrentlyPlayingAsync()
        {
            try
            {
                var response = await httpClient.GetAsync("https://api.spotify.com/v1/me/player/currently-playing");
                
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    // No playback currently active
                    return null;
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Failed to get currently playing: {response.StatusCode}");
                    return null;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(jsonContent);
                
                var isPlaying = jsonDoc.RootElement.GetProperty("is_playing").GetBoolean();
                var progressMs = jsonDoc.RootElement.GetProperty("progress_ms").GetInt32();
                
                var itemElement = jsonDoc.RootElement.GetProperty("item");
                SpotifyTrack? track = null;
                
                if (itemElement.ValueKind != JsonValueKind.Null)
                {
                    var trackId = itemElement.GetProperty("id").GetString() ?? "";
                    var trackName = itemElement.GetProperty("name").GetString() ?? "";
                    var trackUri = itemElement.GetProperty("uri").GetString() ?? "";
                    var durationMs = itemElement.GetProperty("duration_ms").GetInt32();
                    
                    var artists = new List<string>();
                    var artistsArray = itemElement.GetProperty("artists").EnumerateArray();
                    foreach (var artist in artistsArray)
                    {
                        artists.Add(artist.GetProperty("name").GetString() ?? "");
                    }
                    
                    track = new SpotifyTrack
                    {
                        Id = trackId,
                        Name = trackName,
                        Uri = trackUri,
                        DurationMs = durationMs,
                        Artists = artists
                    };
                }

                return new SpotifyCurrentlyPlaying
                {
                    Item = track,
                    IsPlaying = isPlaying,
                    ProgressMs = progressMs
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting currently playing: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Set repeat mode for playback
        /// </summary>
        /// <param name="state">off, track, context</param>
        /// <param name="deviceId">Device ID</param>
        public async Task<bool> SetRepeatAsync(string state = "context", string? deviceId = null)
        {
            try
            {
                var url = $"https://api.spotify.com/v1/me/player/repeat?state={state}";
                if (!string.IsNullOrEmpty(deviceId))
                {
                    url += $"&device_id={deviceId}";
                }

                var response = await httpClient.PutAsync(url, null);
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Successfully set repeat mode to: {state}");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Failed to set repeat mode: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting repeat mode: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
