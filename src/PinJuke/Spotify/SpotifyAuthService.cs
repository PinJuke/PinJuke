using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;

namespace PinJuke.Spotify
{
    /// <summary>
    /// Implementation of Spotify authentication service using OAuth2 flow
    /// </summary>
    public class SpotifyAuthService : ISpotifyAuthService, IDisposable
    {
        private readonly SpotifyConfig _config;
        private readonly HttpClient _httpClient;
        private SpotifyAuthResult? _currentAuth;

        public SpotifyAuthResult? CurrentAuth => _currentAuth;
        public bool IsAuthenticated => _currentAuth?.HasValidToken ?? false;

        public event EventHandler<SpotifyAuthResult>? AuthenticationChanged;

        private const string SPOTIFY_AUTH_URL = "https://accounts.spotify.com/authorize";
        private const string SPOTIFY_TOKEN_URL = "https://accounts.spotify.com/api/token";

        public SpotifyAuthService(SpotifyConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClient = new HttpClient();
        }

        public async Task<SpotifyAuthResult> AuthenticateAsync()
        {
            try
            {
                // Generate state for security
                var state = Guid.NewGuid().ToString();
                
                // Build authorization URL
                var authUrl = BuildAuthorizationUrl(state);
                
                // Open browser for user authentication
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });

                // Start local HTTP server to capture callback
                var authCode = await StartCallbackServerAsync(state);
                
                if (string.IsNullOrEmpty(authCode))
                {
                    var failResult = new SpotifyAuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Authorization was cancelled or failed"
                    };
                    
                    _currentAuth = failResult;
                    AuthenticationChanged?.Invoke(this, failResult);
                    return failResult;
                }

                // Exchange authorization code for access token
                var tokenResult = await ExchangeCodeForTokenAsync(authCode);
                
                _currentAuth = tokenResult;
                AuthenticationChanged?.Invoke(this, tokenResult);
                
                return tokenResult;
            }
            catch (Exception ex)
            {
                var errorResult = new SpotifyAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
                
                _currentAuth = errorResult;
                AuthenticationChanged?.Invoke(this, errorResult);
                return errorResult;
            }
        }

        public async Task<SpotifyAuthResult> RefreshTokenAsync()
        {
            if (_currentAuth?.RefreshToken == null)
            {
                return await AuthenticateAsync();
            }

            try
            {
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "refresh_token"),
                    new("refresh_token", _currentAuth.RefreshToken)
                };

                var request = new HttpRequestMessage(HttpMethod.Post, SPOTIFY_TOKEN_URL)
                {
                    Content = new FormUrlEncodedContent(formData)
                };

                // Add Basic Auth header
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ClientId}:{_config.ClientSecret}"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new SpotifyAuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Token refresh failed: {responseContent}"
                    };
                }

                var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                var result = new SpotifyAuthResult
                {
                    IsSuccess = true,
                    AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
                    RefreshToken = tokenData.TryGetProperty("refresh_token", out var refreshProp) 
                        ? refreshProp.GetString() ?? _currentAuth.RefreshToken 
                        : _currentAuth.RefreshToken,
                    ExpiresAt = DateTime.Now.AddSeconds(tokenData.GetProperty("expires_in").GetInt32()),
                    Scopes = _currentAuth.Scopes
                };

                _currentAuth = result;
                AuthenticationChanged?.Invoke(this, result);
                
                return result;
            }
            catch (Exception ex)
            {
                var errorResult = new SpotifyAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
                
                _currentAuth = errorResult;
                AuthenticationChanged?.Invoke(this, errorResult);
                return errorResult;
            }
        }

        public Task LogoutAsync()
        {
            _currentAuth = null;
            AuthenticationChanged?.Invoke(this, new SpotifyAuthResult { IsSuccess = false, ErrorMessage = "Logged out" });
            return Task.CompletedTask;
        }

        public void SetAuthResult(SpotifyAuthResult authResult)
        {
            _currentAuth = authResult;
            AuthenticationChanged?.Invoke(this, authResult);
        }

        private string BuildAuthorizationUrl(string state)
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["client_id"] = _config.ClientId;
            queryParams["response_type"] = "code";
            queryParams["redirect_uri"] = _config.RedirectUri;
            queryParams["state"] = state;
            queryParams["scope"] = _config.ScopeString;
            queryParams["show_dialog"] = "true";

            return $"{SPOTIFY_AUTH_URL}?{queryParams}";
        }

        private async Task<string?> StartCallbackServerAsync(string expectedState)
        {
            var listener = new HttpListener();
            var redirectUri = _config.RedirectUri.TrimEnd('/') + "/";
            listener.Prefixes.Add(redirectUri);
            
            try
            {
                listener.Start();
                
                // Wait for the callback with a timeout
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // 5 minute timeout
                
                while (!timeoutCts.Token.IsCancellationRequested)
                {
                    var contextTask = listener.GetContextAsync();
                    var timeoutTask = Task.Delay(Timeout.Infinite, timeoutCts.Token);
                    
                    var completedTask = await Task.WhenAny(contextTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        // Timeout occurred
                        return null;
                    }
                    
                    var context = await contextTask;
                    var request = context.Request;
                    var response = context.Response;
                    
                    try
                    {
                        // Extract query parameters
                        var query = request.Url?.Query;
                        if (string.IsNullOrEmpty(query))
                        {
                            await SendResponseAsync(response, "No query parameters received", 400);
                            continue;
                        }
                        
                        var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                        var state = queryParams["state"];
                        var code = queryParams["code"];
                        var error = queryParams["error"];
                        
                        if (!string.IsNullOrEmpty(error))
                        {
                            await SendResponseAsync(response, $"Authorization failed: {error}", 400);
                            return null;
                        }
                        
                        if (state != expectedState)
                        {
                            await SendResponseAsync(response, "Invalid state parameter", 400);
                            return null;
                        }
                        
                        if (string.IsNullOrEmpty(code))
                        {
                            await SendResponseAsync(response, "No authorization code received", 400);
                            continue;
                        }
                        
                        // Success!
                        await SendResponseAsync(response, "Authorization successful! You can close this window.", 200);
                        return code;
                    }
                    finally
                    {
                        response.Close();
                    }
                }
                
                return null; // Timeout
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Callback server error: {ex.Message}");
                return null;
            }
            finally
            {
                listener.Stop();
                listener.Close();
            }
        }
        
        private async Task SendResponseAsync(HttpListenerResponse response, string message, int statusCode)
        {
            response.StatusCode = statusCode;
            response.ContentType = "text/html; charset=utf-8";
            
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>PinJuke Spotify Authorization</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; margin-top: 50px; }}
        .success {{ color: green; }}
        .error {{ color: red; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>PinJuke Spotify Authorization</h1>
        <p class=""{(statusCode == 200 ? "success" : "error")}"">
            {message}
        </p>
        {(statusCode == 200 ? "<p>You may now return to PinJuke.</p>" : "")}
    </div>
</body>
</html>";

            var buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task<SpotifyAuthResult> ExchangeCodeForTokenAsync(string authCode)
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "authorization_code"),
                new("code", authCode),
                new("redirect_uri", _config.RedirectUri)
            };

            var request = new HttpRequestMessage(HttpMethod.Post, SPOTIFY_TOKEN_URL)
            {
                Content = new FormUrlEncodedContent(formData)
            };

            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ClientId}:{_config.ClientSecret}"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new SpotifyAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Token exchange failed: {responseContent}"
                };
            }

            var tokenData = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return new SpotifyAuthResult
            {
                IsSuccess = true,
                AccessToken = tokenData.GetProperty("access_token").GetString() ?? string.Empty,
                RefreshToken = tokenData.TryGetProperty("refresh_token", out var refreshProp) ? refreshProp.GetString() ?? string.Empty : string.Empty,
                ExpiresAt = DateTime.Now.AddSeconds(tokenData.GetProperty("expires_in").GetInt32()),
                Scopes = tokenData.TryGetProperty("scope", out var scopeProp) ? scopeProp.GetString()?.Split(' ') ?? Array.Empty<string>() : Array.Empty<string>()
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
