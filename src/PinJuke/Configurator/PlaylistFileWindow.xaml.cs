using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PinJuke.Configurator
{
    public record PlaylistFileFinishEventData(string FileName);

    public partial class PlaylistFileWindow : Window, IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<PlaylistFileFinishEventData>? FinishEvent;

        public string PlaylistDir
        {
            get => $@"{Configuration.ConfigPath.CONFIG_PLAYLIST_DIRECTORY_PATH}\";
        }

        public string PlaylistExt
        {
            get => ".ini";
        }

        private string fileName = "playlist";
        public string FileName
        {
            get => fileName;
            set
            {
                this.SetField(ref fileName, value);
            }
        }

        public PlaylistFileWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            FinishEvent?.Invoke(this, new PlaylistFileFinishEventData(FileName));
            Close();
        }
    }
}
