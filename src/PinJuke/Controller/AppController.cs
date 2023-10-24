using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Controller
{
    public class AppController : IDisposable
    {
        private readonly MainModel mainModel;

        private Playlist.Scanner? scanner;

        public AppController(MainModel mainModel)
        {
            this.mainModel = mainModel;
        }

        public void Dispose()
        {
            DisposeScanner();
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

        public void Scan()
        {
            DisposeScanner();
            scanner = new Playlist.Scanner(mainModel.Configuration.Player.MusicPath);
            scanner.RunWorkerCompleted += Scanner_RunWorkerCompleted;
            scanner.RunWorkerAsync();
        }

        private void Scanner_RunWorkerCompleted(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }
            mainModel.RootDirectory = (Playlist.FileNode?)e.Result;
        }

    }
}
