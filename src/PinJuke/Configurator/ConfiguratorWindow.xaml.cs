using PinJuke.Configurator.Factory;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PinJuke.Configurator
{
    public partial class ConfiguratorWindow : Window, MediaPathProvider
    {
        public event EventHandler<string> RunPlaylistConfigEvent;

        protected GlobalGroupControlFactory GlobalGroupControlFactory { get; }
        protected PlaylistGroupControlFactory PlaylistGroupControlFactory { get; }

        private IniDocumentTabItem globalTabItem;

        public string DocumentationText
        {
            get => Strings.LabelDocumentation;
        }

        public string DocumentationLink
        {
            get => "https://pinjuke.github.io/PinJuke/";
        }

        public string AddPlaylistLabelText
        {
            get => Strings.LabelAddNewPlaylist;
        }

        public string SaveAllLabelText
        {
            get => Strings.LabelSaveAll;
        }

        public string RunCurrentPlaylistLabelText
        {
            get => Strings.LabelRunCurrentPlaylist;
        }

        public ConfiguratorWindow()
        {
            DataContext = this;
            InitializeComponent();

            var parser = new Configuration.Parser();
            var pinUpReader = new PinUpPlayerIniReader(new()
            {
                Directory.GetCurrentDirectory(),
                Path.Join(Path.GetPathRoot(Directory.GetCurrentDirectory()), "vPinball"),
                Path.Join("C:", "vPinball"),
            });

            GlobalGroupControlFactory = new(parser, pinUpReader);
            PlaylistGroupControlFactory = new(parser, this);

            globalTabItem = new IniDocumentTabItem(
                GlobalGroupControlFactory,
                Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH,
                Configuration.ConfigPath.TEMPLATE_GLOBAL_FILE_PATH
            );
            Tabs.Items.Add(globalTabItem);

            var playlistDir = Configuration.ConfigPath.CONFIG_PLAYLIST_DIRECTORY_PATH;
            var directoryInfo = new DirectoryInfo(playlistDir);
            foreach (var fileInfo in directoryInfo.GetFiles("*.ini"))
            {
                var playlistTabItem = new IniDocumentTabItem(
                    PlaylistGroupControlFactory,
                    Path.Combine(playlistDir, fileInfo.Name),
                    Configuration.ConfigPath.TEMPLATE_PLAYLIST_FILE_PATH
                );
                Tabs.Items.Add(playlistTabItem);
            }

            UpdateTabsSelection();
        }

        public string GetMediaPath()
        {
            var mediaPathControl = (PathControl)globalTabItem.GroupControl.GetChildByName(GlobalGroupControlFactory.MEDIA_PATH_CONTROL);
            return mediaPathControl.FullPath;
        }

        private void SaveButton_Clicked(object sender, RoutedEventArgs e)
        {
            foreach (var item in Tabs.Items)
            {
                var tabItem = (IniDocumentTabItem)item;
                try
                {
                    tabItem.SaveToFile();
                }
                catch (IOException)
                {
                    MessageBox.Show(string.Format(Strings.ErrorWritingFile, tabItem.FilePath), AppDomain.CurrentDomain.FriendlyName);
                }
            }
        }

        private void AddPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.DefaultExt = ".ini"; // Default file extension
            dialog.Filter = $"{Strings.IniFile}|*.ini"; // Filter files by extension
            dialog.InitialDirectory = Path.GetFullPath(Configuration.ConfigPath.CONFIG_PLAYLIST_DIRECTORY_PATH);
            
            bool? result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }

            var path = dialog.FileName;
            var workingDir = Directory.GetCurrentDirectory();
            path = System.IO.Path.GetRelativePath(workingDir, path);

            foreach (var item in Tabs.Items)
            {
                var tabItem = (IniDocumentTabItem)item;
                if (tabItem.FilePath.ToLowerInvariant() == path.ToLowerInvariant())
                {
                    Tabs.SelectedItem = tabItem;
                    return;
                }
            }

            var playlistTabItem = new IniDocumentTabItem(
                PlaylistGroupControlFactory,
                path,
                Configuration.ConfigPath.TEMPLATE_PLAYLIST_FILE_PATH
            );
            Tabs.Items.Add(playlistTabItem);
            Tabs.SelectedItem = playlistTabItem;
        }

        private void RunButton_Clicked(object sender, RoutedEventArgs e)
        {
            var tabItem = (IniDocumentTabItem)Tabs.SelectedItem;
            RunPlaylistConfigEvent?.Invoke(this, tabItem.FilePath);
        }

        private void DocumentationLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = DocumentationLink,
                UseShellExecute = true
            });
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTabsSelection();
        }

        private void UpdateTabsSelection()
        {
            RunButton.IsEnabled = Tabs.SelectedIndex >= 1;
        }
    }
}
