using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Implementation of Spotify Web API service
    /// </summary>
    public class SpotifyService : ISpotifyService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private SpotifyConfig? _config;
        private ISpotifyAuthService? _authService;

        private const string SPOTIFY_API_BASE = "https://api.spotify.com/v1";

        public ISpotifyAuthService AuthService => _authService ?? throw new InvalidOperationException("Service not initialized");
        public bool IsConnected => _authService?.IsAuthenticated ?? false;

        public event EventHandler<bool>? ConnectionStateChanged;

        public SpotifyService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PinJuke/1.0");
        }

        public Task InitializeAsync(SpotifyConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _authService = new SpotifyAuthService(config);
            _authService.AuthenticationChanged += OnAuthenticationChanged;
            
            return Task.CompletedTask;
        }

        private void OnAuthenticationChanged(object? sender, SpotifyAuthResult authResult)
        {
            ConnectionStateChanged?.Invoke(this, authResult.HasValidToken);
        }

        public async Task<List<SpotifyPlaylist>> GetUserPlaylistsAsync()
        {
            EnsureAuthenticated();

            try
            {
                var playlists = new List<SpotifyPlaylist>();
                var url = $"{SPOTIFY_API_BASE}/me/playlists?limit=50";

                do
                {
                    var response = await MakeAuthenticatedRequestAsync(url);
                    if (response == null) break;

                    var playlistsData = response.Value.GetProperty("items");
                    foreach (var playlistJson in playlistsData.EnumerateArray())
                    {
                        var playlist = ParsePlaylist(playlistJson);
                        if (playlist != null)
                        {
                            playlists.Add(playlist);
                        }
                    }

                    // Check for next page
                    if (response.Value.TryGetProperty("next", out var nextProp) && !nextProp.ValueKind.Equals(JsonValueKind.Null))
                    {
                        var nextUrl = nextProp.GetString();
                        if (nextUrl != null)
                        {
                            url = nextUrl;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    
                } while (playlists.Count < (_config?.MaxTracksPerPlaylist ?? 500));

                return playlists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching playlists: {ex.Message}");
                return new List<SpotifyPlaylist>();
            }
        }

        public async Task<SpotifyPlaylist?> GetPlaylistAsync(string playlistId)
        {
            EnsureAuthenticated();

            try
            {
                var url = $"{SPOTIFY_API_BASE}/playlists/{playlistId}";
                var response = await MakeAuthenticatedRequestAsync(url);
                
                if (response == null) return null;

                var playlist = ParsePlaylist(response.Value);
                if (playlist == null) return null;

                // Get tracks
                var tracksUrl = $"{SPOTIFY_API_BASE}/playlists/{playlistId}/tracks?limit=50";
                var tracks = new List<SpotifyTrack>();

                do
                {
                    if (tracksUrl == null) break;
                    
                    var tracksResponse = await MakeAuthenticatedRequestAsync(tracksUrl);
                    if (tracksResponse == null) break;

                    var tracksData = tracksResponse.Value.GetProperty("items");
                    foreach (var trackItem in tracksData.EnumerateArray())
                    {
                        if (trackItem.TryGetProperty("track", out var trackJson) && 
                            trackJson.ValueKind != JsonValueKind.Null)
                        {
                            var track = ParseTrack(trackJson);
                            if (track != null)
                            {
                                tracks.Add(track);
                            }
                        }
                    }

                    // Check for next page
                    if (tracksResponse.Value.TryGetProperty("next", out var nextProp) && 
                        !nextProp.ValueKind.Equals(JsonValueKind.Null))
                    {
                        tracksUrl = nextProp.GetString();
                    }
                    else
                    {
                        break;
                    }
                    
                } while (tracks.Count < (_config?.MaxTracksPerPlaylist ?? 500));

                playlist.Tracks = tracks;
                playlist.TrackCount = tracks.Count;

                return playlist;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching playlist {playlistId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<SpotifyTrack>> GetUserSavedTracksAsync()
        {
            EnsureAuthenticated();

            try
            {
                var tracks = new List<SpotifyTrack>();
                var url = $"{SPOTIFY_API_BASE}/me/tracks?limit=50";

                do
                {
                    if (url == null) break;
                    
                    var response = await MakeAuthenticatedRequestAsync(url);
                    if (response == null) break;

                    var tracksData = response.Value.GetProperty("items");
                    foreach (var trackItem in tracksData.EnumerateArray())
                    {
                        if (trackItem.TryGetProperty("track", out var trackJson))
                        {
                            var track = ParseTrack(trackJson);
                            if (track != null)
                            {
                                tracks.Add(track);
                            }
                        }
                    }

                    // Check for next page
                    if (response.Value.TryGetProperty("next", out var nextProp) && 
                        !nextProp.ValueKind.Equals(JsonValueKind.Null))
                    {
                        var nextUrl = nextProp.GetString();
                        if (nextUrl != null)
                        {
                            url = nextUrl;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    
                } while (tracks.Count < (_config?.MaxTracksPerPlaylist ?? 500));

                return tracks;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching saved tracks: {ex.Message}");
                return new List<SpotifyTrack>();
            }
        }

        public async Task<List<SpotifyTrack>> SearchTracksAsync(string query, int limit = 50)
        {
            EnsureAuthenticated();

            try
            {
                var encodedQuery = System.Web.HttpUtility.UrlEncode(query);
                var url = $"{SPOTIFY_API_BASE}/search?q={encodedQuery}&type=track&limit={Math.Min(limit, 50)}";
                
                var response = await MakeAuthenticatedRequestAsync(url);
                if (response == null) return new List<SpotifyTrack>();

                var tracks = new List<SpotifyTrack>();
                if (response.Value.TryGetProperty("tracks", out var tracksObj) &&
                    tracksObj.TryGetProperty("items", out var tracksArray))
                {
                    foreach (var trackJson in tracksArray.EnumerateArray())
                    {
                        var track = ParseTrack(trackJson);
                        if (track != null)
                        {
                            tracks.Add(track);
                        }
                    }
                }

                return tracks;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching tracks: {ex.Message}");
                return new List<SpotifyTrack>();
            }
        }

        public async Task<string?> GetTrackPlayUrlAsync(string trackId)
        {
            EnsureAuthenticated();

            try
            {
                var url = $"{SPOTIFY_API_BASE}/tracks/{trackId}";
                var response = await MakeAuthenticatedRequestAsync(url);
                
                if (response == null) return null;

                // Extract preview URL if available
                if (response.Value.TryGetProperty("preview_url", out var previewProp) &&
                    previewProp.ValueKind != JsonValueKind.Null)
                {
                    return previewProp.GetString();
                }

                return null; // No preview available
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting track play URL for {trackId}: {ex.Message}");
                return null;
            }
        }

        public async Task<SpotifyCurrentlyPlaying?> GetCurrentlyPlayingAsync()
        {
            EnsureAuthenticated();

            try
            {
                var url = $"{SPOTIFY_API_BASE}/me/player/currently-playing";
                var response = await MakeAuthenticatedRequestAsync(url);
                
                if (response == null) return null;

                var currentlyPlaying = new SpotifyCurrentlyPlaying();
                
                // Parse device
                if (response.Value.TryGetProperty("device", out var deviceProp))
                {
                    currentlyPlaying.Device = ParseDevice(deviceProp);
                }

                // Parse basic playback info
                if (response.Value.TryGetProperty("is_playing", out var isPlayingProp))
                {
                    currentlyPlaying.IsPlaying = isPlayingProp.GetBoolean();
                }

                if (response.Value.TryGetProperty("progress_ms", out var progressProp) && 
                    progressProp.ValueKind != JsonValueKind.Null)
                {
                    currentlyPlaying.ProgressMs = progressProp.GetInt32();
                }

                if (response.Value.TryGetProperty("timestamp", out var timestampProp))
                {
                    currentlyPlaying.Timestamp = timestampProp.GetInt64();
                }

                if (response.Value.TryGetProperty("currently_playing_type", out var typeProp))
                {
                    currentlyPlaying.CurrentlyPlayingType = typeProp.GetString() ?? "track";
                }

                // Parse shuffle and repeat states
                if (response.Value.TryGetProperty("shuffle_state", out var shuffleProp))
                {
                    currentlyPlaying.ShuffleState = shuffleProp.GetBoolean();
                }

                if (response.Value.TryGetProperty("repeat_state", out var repeatProp))
                {
                    currentlyPlaying.RepeatState = repeatProp.GetString() ?? "off";
                }

                // Parse currently playing item (track)
                if (response.Value.TryGetProperty("item", out var itemProp) && 
                    itemProp.ValueKind != JsonValueKind.Null)
                {
                    currentlyPlaying.Item = ParseTrack(itemProp);
                }

                // Parse context (playlist, album, etc.)
                if (response.Value.TryGetProperty("context", out var contextProp) && 
                    contextProp.ValueKind != JsonValueKind.Null)
                {
                    currentlyPlaying.Context = ParseContext(contextProp);
                }

                // Parse actions
                if (response.Value.TryGetProperty("actions", out var actionsProp))
                {
                    currentlyPlaying.Actions = ParseActions(actionsProp);
                }

                return currentlyPlaying;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting currently playing track: {ex.Message}");
                return null;
            }
        }

        private SpotifyDevice? ParseDevice(JsonElement deviceJson)
        {
            try
            {
                return new SpotifyDevice
                {
                    Id = deviceJson.TryGetProperty("id", out var idProp) ? (idProp.GetString() ?? string.Empty) : string.Empty,
                    Name = deviceJson.TryGetProperty("name", out var nameProp) ? (nameProp.GetString() ?? string.Empty) : string.Empty,
                    Type = deviceJson.TryGetProperty("type", out var typeProp) ? (typeProp.GetString() ?? string.Empty) : string.Empty,
                    IsActive = deviceJson.TryGetProperty("is_active", out var activeProp) && activeProp.GetBoolean(),
                    IsRestricted = deviceJson.TryGetProperty("is_restricted", out var restrictedProp) && restrictedProp.GetBoolean(),
                    IsPrivateSession = deviceJson.TryGetProperty("is_private_session", out var privateProp) && privateProp.GetBoolean(),
                    VolumePercent = deviceJson.TryGetProperty("volume_percent", out var volumeProp) && volumeProp.ValueKind != JsonValueKind.Null ? volumeProp.GetInt32() : 0,
                    SupportsVolume = deviceJson.TryGetProperty("supports_volume", out var supportsVolumeProp) && supportsVolumeProp.GetBoolean()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing device: {ex.Message}");
                return null;
            }
        }

        private SpotifyPlaybackContext? ParseContext(JsonElement contextJson)
        {
            try
            {
                var context = new SpotifyPlaybackContext();

                if (contextJson.TryGetProperty("type", out var typeProp))
                {
                    context.Type = typeProp.GetString() ?? string.Empty;
                }

                if (contextJson.TryGetProperty("uri", out var uriProp))
                {
                    context.Uri = uriProp.GetString() ?? string.Empty;
                }

                if (contextJson.TryGetProperty("href", out var hrefProp))
                {
                    context.Href = hrefProp.GetString() ?? string.Empty;
                }

                if (contextJson.TryGetProperty("external_urls", out var urlsProp))
                {
                    foreach (var urlProp in urlsProp.EnumerateObject())
                    {
                        if (urlProp.Value.ValueKind == JsonValueKind.String)
                        {
                            context.ExternalUrls[urlProp.Name] = urlProp.Value.GetString() ?? string.Empty;
                        }
                    }
                }

                return context;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing context: {ex.Message}");
                return null;
            }
        }

        private SpotifyPlaybackActions? ParseActions(JsonElement actionsJson)
        {
            try
            {
                return new SpotifyPlaybackActions
                {
                    InterruptingPlayback = actionsJson.TryGetProperty("interrupting_playback", out var interruptProp) && interruptProp.GetBoolean(),
                    Pausing = actionsJson.TryGetProperty("pausing", out var pauseProp) && pauseProp.GetBoolean(),
                    Resuming = actionsJson.TryGetProperty("resuming", out var resumeProp) && resumeProp.GetBoolean(),
                    Seeking = actionsJson.TryGetProperty("seeking", out var seekProp) && seekProp.GetBoolean(),
                    SkippingNext = actionsJson.TryGetProperty("skipping_next", out var nextProp) && nextProp.GetBoolean(),
                    SkippingPrev = actionsJson.TryGetProperty("skipping_prev", out var prevProp) && prevProp.GetBoolean(),
                    TogglingRepeatContext = actionsJson.TryGetProperty("toggling_repeat_context", out var repeatContextProp) && repeatContextProp.GetBoolean(),
                    TogglingShufflee = actionsJson.TryGetProperty("toggling_shuffle", out var shuffleProp) && shuffleProp.GetBoolean(),
                    TogglingRepeatTrack = actionsJson.TryGetProperty("toggling_repeat_track", out var repeatTrackProp) && repeatTrackProp.GetBoolean(),
                    TransferringPlayback = actionsJson.TryGetProperty("transferring_playback", out var transferProp) && transferProp.GetBoolean()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing actions: {ex.Message}");
                return null;
            }
        }

        private async Task<JsonElement?> MakeAuthenticatedRequestAsync(string url)
        {
            if (_authService?.CurrentAuth == null || !_authService.IsAuthenticated)
            {
                return null;
            }

            // Refresh token if needed
            if (_authService.CurrentAuth.IsExpired)
            {
                await _authService.RefreshTokenAsync();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.CurrentAuth.AccessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Spotify API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(content);
        }

        private SpotifyPlaylist? ParsePlaylist(JsonElement playlistJson)
        {
            try
            {
                var playlist = new SpotifyPlaylist
                {
                    Id = playlistJson.GetProperty("id").GetString() ?? string.Empty,
                    Name = playlistJson.GetProperty("name").GetString() ?? string.Empty,
                    Description = playlistJson.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                    TrackCount = playlistJson.GetProperty("tracks").GetProperty("total").GetInt32(),
                    IsPublic = playlistJson.TryGetProperty("public", out var publicProp) && publicProp.GetBoolean(),
                    IsCollaborative = playlistJson.TryGetProperty("collaborative", out var collabProp) && collabProp.GetBoolean(),
                    Uri = playlistJson.GetProperty("uri").GetString() ?? string.Empty,
                };

                if (playlistJson.TryGetProperty("owner", out var ownerProp))
                {
                    playlist.Owner = ownerProp.GetProperty("display_name").GetString() ?? ownerProp.GetProperty("id").GetString() ?? string.Empty;
                    playlist.OwnerId = ownerProp.GetProperty("id").GetString() ?? string.Empty;
                }

                if (playlistJson.TryGetProperty("external_urls", out var urlsProp) &&
                    urlsProp.TryGetProperty("spotify", out var spotifyUrlProp))
                {
                    playlist.ExternalUrl = spotifyUrlProp.GetString() ?? string.Empty;
                }

                if (playlistJson.TryGetProperty("images", out var imagesProp))
                {
                    playlist.ImageUrls = imagesProp.EnumerateArray()
                        .Select(img => img.GetProperty("url").GetString())
                        .Where(url => !string.IsNullOrEmpty(url))
                        .Cast<string>()
                        .ToList();
                }

                return playlist;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing playlist: {ex.Message}");
                return null;
            }
        }

        private SpotifyTrack? ParseTrack(JsonElement trackJson)
        {
            try
            {
                var track = new SpotifyTrack
                {
                    Id = trackJson.GetProperty("id").GetString() ?? string.Empty,
                    Name = trackJson.GetProperty("name").GetString() ?? string.Empty,
                    DurationMs = trackJson.GetProperty("duration_ms").GetInt32(),
                    Uri = trackJson.GetProperty("uri").GetString() ?? string.Empty,
                    IsPlayable = !trackJson.TryGetProperty("is_playable", out var playableProp) || playableProp.GetBoolean(),
                    Popularity = trackJson.TryGetProperty("popularity", out var popProp) ? popProp.GetInt32() : 0
                };

                if (trackJson.TryGetProperty("preview_url", out var previewProp) && 
                    previewProp.ValueKind != JsonValueKind.Null)
                {
                    track.PreviewUrl = previewProp.GetString() ?? string.Empty;
                }

                if (trackJson.TryGetProperty("external_urls", out var urlsProp) &&
                    urlsProp.TryGetProperty("spotify", out var spotifyUrlProp))
                {
                    track.ExternalUrl = spotifyUrlProp.GetString() ?? string.Empty;
                }

                if (trackJson.TryGetProperty("artists", out var artistsProp))
                {
                    var artists = artistsProp.EnumerateArray()
                        .Select(artist => artist.GetProperty("name").GetString())
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Cast<string>()
                        .ToList();
                    
                    track.Artists = artists;
                    track.Artist = artists.FirstOrDefault() ?? string.Empty;
                }

                if (trackJson.TryGetProperty("album", out var albumProp))
                {
                    track.Album = albumProp.GetProperty("name").GetString() ?? string.Empty;
                    
                    // Parse album release date if available
                    if (albumProp.TryGetProperty("release_date", out var releaseProp))
                    {
                        track.AlbumReleaseDate = releaseProp.GetString() ?? string.Empty;
                    }
                    
                    if (albumProp.TryGetProperty("release_date_precision", out var precisionProp))
                    {
                        track.AlbumReleaseDatePrecision = precisionProp.GetString() ?? string.Empty;
                    }
                    
                    // Parse album images if available
                    if (albumProp.TryGetProperty("images", out var imagesProp))
                    {
                        track.AlbumImageUrls = imagesProp.EnumerateArray()
                            .Select(img => img.GetProperty("url").GetString())
                            .Where(url => !string.IsNullOrEmpty(url))
                            .Cast<string>()
                            .ToList();
                    }
                }

                return track;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing track: {ex.Message}");
                return null;
            }
        }

        private void EnsureAuthenticated()
        {
            if (_authService == null)
            {
                throw new InvalidOperationException("Spotify service not initialized");
            }

            if (!_authService.IsAuthenticated)
            {
                throw new InvalidOperationException("Not authenticated with Spotify");
            }
        }

        public void Dispose()
        {
            if (_authService != null)
            {
                _authService.AuthenticationChanged -= OnAuthenticationChanged;
                (_authService as IDisposable)?.Dispose();
            }
            _httpClient?.Dispose();
        }
    }
}
