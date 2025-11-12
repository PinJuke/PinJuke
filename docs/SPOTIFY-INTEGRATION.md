![Jukebox](images/jukebox-header.webp)

- [Start](index.md)
- [Features](FEATURES.md)
- [Installation](INSTALLATION.md)
- [Setup Wizard](ONBOARDING.md)
- [Configuration](CONFIGURATION.md)
- [Theme Video Clips](THEME-VIDEOS.md)
- [Spotify Integration](SPOTIFY-INTEGRATION.md)
- [Controls](CONTROLS.md)
- [Run a Playlist File](RUN.md)
- [Pinup Popper](PINUP-POPPER.md)
- [FAQ](FAQ.md)

# Spotify Integration

PinJuke now features full Spotify Web API integration, allowing you to stream music directly from Spotify and display real-time album artwork and track information on your pinball cabinet displays.

## Features

- **Real-time Spotify Playback**: Stream music directly from your Spotify Premium account
- **Dynamic Album Artwork**: Automatically displays album covers that update as tracks change
- **Responsive Track Updates**: Immediate display updates when songs change (optimized 500ms polling)
- **Device Management**: Automatic detection and selection of Spotify playback devices
- **Playlist Support**: Load and play Spotify playlists through PinJuke's interface
- **Shuffle & Repeat**: Full control over Spotify playback modes
- **Volume Control**: Integrated volume management for Spotify playback

## Prerequisites

### Spotify Requirements
- **Spotify Premium Account** (required for Web API playback control)
- **Spotify Application Registration** (for API access)

### System Requirements
- Internet connection for Spotify API communication
- Modern web browser for initial authentication

## Setup Instructions

### 1. Create a Spotify Application

1. Visit the [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Log in with your Spotify account
3. Click **"Create App"**
4. Fill out the application details:
   - **App name**: `PinJuke` (or your preferred name)
   - **App description**: `Pinball jukebox integration`
   - **Redirect URI**: `http://localhost:8080/callback`
   - **API/SDKs**: Check "Web API"
5. Click **"Save"**
6. Note your **Client ID** and **Client Secret** for configuration

### 2. Configure PinJuke

#### Using the Configurator (Recommended)
1. Launch `PinJuke.exe` to open the Configurator
2. Navigate to the **Global Configuration** section
3. Locate the **Spotify Integration** settings
4. Enter your Spotify application credentials:
   - **Client ID**: Your Spotify app's Client ID
   - **Client Secret**: Your Spotify app's Client Secret
   - **Redirect URI**: `http://127.0.0.1:8888/callback` (must match your Spotify app setting)
5. Configure device settings:
   - **Device ID**: (Optional) Specific Spotify device to control
   - **Default Volume**: Default playback volume (0-100)
6. Save the configuration

#### Manual Configuration
Edit `Configs/PinJuke.global.ini` and add:

```ini
[Spotify]
ClientId=your_client_id_here
ClientSecret=your_client_secret_here
RedirectUri=http://127.0.0.1:8888/callback
DeviceId=
DefaultVolume=75
```

### 3. Initial Authentication

1. Launch PinJuke with a Spotify-enabled playlist configuration
2. When prompted, authorize PinJuke to access your Spotify account
3. Complete the OAuth flow in your web browser
4. Return to PinJuke - authentication will be automatically saved

## Usage

### Playing Spotify Content

#### From Spotify Playlists
1. Configure a playlist with Spotify playlist ID in the Configurator
2. Launch PinJuke with the playlist: `PinJuke.exe "path\to\spotify-playlist.ini"`
3. Use standard PinJuke controls to navigate and play tracks

#### Device Control
- PinJuke will automatically detect available Spotify devices
- If no device is specified, it will use the currently active device
- Use the device refresh button in the Configurator to update the device list

### Display Features

#### Real-time Updates
- **Album Artwork**: Automatically fetches and displays current track's album cover
- **Track Information**: Shows artist, song title, and album information
- **Playback Status**: Indicates play/pause state and track progress

#### Multi-Display Support
- **Backglass Display**: Shows album artwork and track information
- **Playfield Display**: Can show visualizations or additional track info
- **DMD Display**: Shows compact track information

## Configuration Options

### Global Spotify Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `ClientId` | Spotify application Client ID | (required) |
| `ClientSecret` | Spotify application Client Secret | (required) |
| `RedirectUri` | OAuth redirect URL | `http://127.0.0.1:8888/callback` |
| `DeviceId` | Specific Spotify device ID (optional) | (auto-detect) |
| `DefaultVolume` | Default playback volume (0-100) | `75` |

### Playlist-Specific Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `SpotifyPlaylistId` | Spotify playlist ID to load | (none) |
| `ShufflePlaylist` | Enable playlist shuffling | `false` |

## Troubleshooting

### Common Issues

#### Authentication Problems
- **Error**: "Invalid client credentials"
  - **Solution**: Verify your Client ID and Client Secret in the configuration
  - **Check**: Ensure your Spotify app settings match your PinJuke configuration

#### Playback Issues
- **Error**: "No active device found"
  - **Solution**: Start Spotify on any device (phone, computer) to create an active session
  - **Alternative**: Specify a Device ID in the configuration

- **Error**: "Premium required"
  - **Solution**: Spotify Web API requires a Premium subscription for playback control

#### Connection Problems
- **Error**: "Failed to connect to Spotify"
  - **Check**: Internet connection is active
  - **Verify**: Firewall is not blocking PinJuke
  - **Test**: Try refreshing authentication tokens

### Performance Optimization

#### Reducing API Calls
- Track updates use optimized 500ms polling to balance responsiveness with API rate limits
- Album artwork is cached locally to reduce repeated downloads
- Device discovery is performed on-demand rather than continuously

#### Memory Management
- Album artwork is automatically cleaned up when tracks change
- API response caching reduces memory usage and improves performance

## Advanced Configuration

### Custom Device Selection
To target a specific Spotify device:

1. Open the Configurator
2. Navigate to Spotify settings
3. Click "Refresh Devices" to discover available devices
4. Select your preferred device from the dropdown
5. The Device ID will be automatically populated

### Playlist Import
To use an existing Spotify playlist:

1. Open your desired playlist in the Spotify web app
2. Copy the playlist ID from the URL (e.g., `37i9dQZF1DXcBWIGoYBM5M`)
3. Enter this ID in your PinJuke playlist configuration
4. Save and restart PinJuke

## Security Notes

- **Client Secret**: Keep your Spotify Client Secret secure and never share it publicly
- **Authentication Tokens**: PinJuke stores refresh tokens locally in encrypted form
- **Network Security**: All Spotify API communication uses HTTPS encryption

## API Rate Limits

Spotify imposes rate limits on API calls:
- **Track Updates**: Optimized to respect rate limits with 500ms polling
- **Device Discovery**: Limited to on-demand requests
- **Playlist Loading**: Cached after initial load

PinJuke automatically handles rate limiting and will adjust polling frequency if limits are reached.

## Support

For Spotify integration issues:
1. Check this documentation for common solutions
2. Verify your Spotify app configuration matches PinJuke settings
3. Test with a simple playlist to isolate configuration issues
4. Check the PinJuke logs for detailed error messages

For Spotify Developer API issues, consult the [Spotify Web API Documentation](https://developer.spotify.com/documentation/web-api/).
