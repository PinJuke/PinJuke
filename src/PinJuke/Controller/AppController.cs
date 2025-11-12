using PinJuke.Audio;
using PinJuke.Configuration;
using PinJuke.Model;
using PinJuke.Playlist;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Controller
{
    public class AppController : IDisposable
    {
        private readonly MainModel mainModel;

        private Playlist.Scanner? scanner;
        private SpotifyScanner? spotifyScanner;

        public AppController(MainModel mainModel)
        {
            this.mainModel = mainModel;
        }

        public void Dispose()
        {
            DisposeScanner();
            DisposeSpotifyScanner();
        }

        private void DisposeScanner()
        {
            if (scanner != null)
            {
                scanner.RunWorkerCompleted -= Scanner_RunWorkerCompleted;
                scanner.CancelAsync();
                scanner = null;
            }
        }

        private void DisposeSpotifyScanner()
        {
            if (spotifyScanner != null)
            {
                spotifyScanner.RunWorkerCompleted -= SpotifyScanner_RunWorkerCompleted;
                spotifyScanner.CancelAsync();
                spotifyScanner = null;
            }
        }

        public void Scan()
        {
            DisposeScanner();
            DisposeSpotifyScanner();

            var playerConfig = mainModel.Configuration.Player;
            
            if (playerConfig.SourceType == PlayerSourceType.SpotifyPlaylist)
            {
                Debug.WriteLine($"AppController: Starting Spotify scan for playlist {playerConfig.SpotifyPlaylistId}");
                
                spotifyScanner = new SpotifyScanner(playerConfig, mainModel.Configuration.Spotify);
                spotifyScanner.RunWorkerCompleted += SpotifyScanner_RunWorkerCompleted;
                spotifyScanner.RunWorkerAsync();
            }
            else
            {
                Debug.WriteLine($"AppController: Starting local file scan for path {playerConfig.MusicPath}");
                
                scanner = new Playlist.Scanner(playerConfig.MusicPath);
                scanner.RunWorkerCompleted += Scanner_RunWorkerCompleted;
                scanner.RunWorkerAsync();
            }
        }

        private void Scanner_RunWorkerCompleted(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Debug.WriteLine("AppController: Local file scan was cancelled");
                return;
            }
            
            if (e.Error != null)
            {
                Debug.WriteLine($"AppController: Local file scan failed: {e.Error.Message}");
                return;
            }

            Debug.WriteLine("AppController: Local file scan completed successfully");
            var scanResult = (ScanResult)e.Result!;
            mainModel.SetScanResult(scanResult);
        }

        private void SpotifyScanner_RunWorkerCompleted(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Debug.WriteLine("AppController: Spotify scan was cancelled");
                return;
            }
            
            if (e.Error != null)
            {
                Debug.WriteLine($"AppController: Spotify scan failed: {e.Error.Message}");
                return;
            }

            Debug.WriteLine("AppController: Spotify scan completed successfully");
            var scanResult = (ScanResult)e.Result!;
            mainModel.SetScanResult(scanResult);
        }
    }
}
