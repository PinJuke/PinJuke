using PinJuke.Configurator.Factory;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using PinJuke.Model;
using PinJuke.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace PinJuke.Configurator
{
    public partial class ConfiguratorWindow : Window, MediaPathProvider
    {
        public event EventHandler<string>? RunPlaylistConfigEvent;
        public event EventHandler? RunOnboardingEvent;

        protected GlobalGroupControlFactory GlobalGroupControlFactory { get; }
        protected PlaylistGroupControlFactory PlaylistGroupControlFactory { get; }

        private IniDocumentTabItem globalTabItem;

        public string DocumentationLink
        {
            get => "https://pinjuke.github.io/PinJuke/";
        }

        public ImageSource AddPlaylistImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\add-outline.svg");
        }

        public ImageSource SaveAllImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\save-outline.svg");
        }

        public ImageSource RunCurrentPlaylistImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\play-outline.svg");
        }

        public ImageSource OnboardingImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\rocket-outline.svg");
        }

        public ConfiguratorWindow()
        {
            DataContext = this;
            InitializeComponent();

            var parser = new Configuration.Parser();
            var pinUpReader = PinUpPlayerIniReader.Create();

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
                    Path.Join(playlistDir, fileInfo.Name),
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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
            ShowAddPlaylist();
        }

        public void ShowAddPlaylist()
        {
            var playlistFileWindow = new PlaylistFileWindow();
            playlistFileWindow.Owner = this;
            playlistFileWindow.FinishEvent += AddPlaylist;
            playlistFileWindow.ShowDialog();
        }

        private void AddPlaylist(object? sender, PlaylistFileFinishEventData eventData)
        {
            var path = $@"{Configuration.ConfigPath.CONFIG_PLAYLIST_DIRECTORY_PATH}\{eventData.FileName}.ini";
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

            // Save the new playlist file to disk.
            playlistTabItem.SaveToFile();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            var tabItem = (IniDocumentTabItem)Tabs.SelectedItem;
            RunPlaylistConfigEvent?.Invoke(this, tabItem.FilePath);
        }

        private void RunOnboarding_Click(object sender, RoutedEventArgs e)
        {
            RunOnboardingEvent?.Invoke(this, EventArgs.Empty);
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
