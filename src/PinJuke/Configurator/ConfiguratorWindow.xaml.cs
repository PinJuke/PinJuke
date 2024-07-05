using PinJuke.Configurator.Factory;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PinJuke.Configurator
{
    public partial class ConfiguratorWindow : Window
    {
        protected GlobalGroupControlFactory GlobalGroupControlFactory { get; }
        protected PlaylistGroupControlFactory PlaylistGroupControlFactory { get; }

        public ConfiguratorWindow()
        {
            InitializeComponent();

            var parser = new Configuration.Parser();

            GlobalGroupControlFactory = new(parser);
            PlaylistGroupControlFactory = new(parser);

            AddTabItems();
        }

        private void AddTabItems()
        {
            var globalTabItem = new IniDocumentTabItem(
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

        }

        private void SaveButton_Clicked(object sender, RoutedEventArgs e)
        {
            foreach (var item in Tabs.Items)
            {
                var tabItem = (IniDocumentTabItem)item;
                tabItem.SaveToFile();
            }
        }

        private void AddPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.DefaultExt = ".ini"; // Default file extension
            dialog.Filter = "Ini file|*.ini"; // Filter files by extension
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
    }
}
