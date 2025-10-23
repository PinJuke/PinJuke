using PinJuke.Configurator.Factory;
using PinJuke.Configurator.View;
using PinJuke.Ini;
using PinJuke.Model;
using PinJuke.Service;
using PinJuke.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;


namespace PinJuke.Configurator
{
    public partial class ConfiguratorWindow : Window, MediaPathProvider, IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<string>? RunPlaylistConfigEvent;
        public event EventHandler? RunOnboardingEvent;

        private readonly UpdateCheckService updateCheckService;
        private readonly Configuration.DistributionInfo distributionInfo;

        protected GlobalGroupControlFactory GlobalGroupControlFactory { get; }
        protected PlaylistGroupControlFactory PlaylistGroupControlFactory { get; }

        private IniDocumentTabItem globalTabItem;

        public string DocumentationLink
        {
            get => "https://pinjuke.github.io/PinJuke/";
        }

        public string ReleasesLink
        {
            get => distributionInfo.DownloadLink;
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

        public ImageSource HelpImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\help-circle-outline.svg");
        }

        private bool updateHintVisible = false;
        public bool UpdateHintVisible
        {
            get => updateHintVisible;
            set
            {
                this.SetField(ref updateHintVisible, value);
            }
        }

        public ConfiguratorWindow(UpdateCheckService updateCheckService, ConfigurationService configurationService)
        {
            this.updateCheckService = updateCheckService;

            DataContext = this;
            InitializeComponent();

            distributionInfo = configurationService.LoadDistributionInfo();

            var parser = new Configuration.Parser();
            var pinUpReader = PinUpPlayerIniReader.Create();

            GlobalGroupControlFactory = new(parser, pinUpReader);
            PlaylistGroupControlFactory = new(parser, this);

            globalTabItem = new IniDocumentTabItem(
                GlobalGroupControlFactory,
                Strings.TabGlobalConfiguration,
                "",
                Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH,
                Configuration.ConfigPath.TEMPLATE_GLOBAL_FILE_PATH
            );
            Tabs.Items.Add(globalTabItem);

            var playlistDir = Configuration.ConfigPath.CONFIG_PLAYLIST_DIRECTORY_PATH;
            var directoryInfo = new DirectoryInfo(playlistDir);
            FileInfo[]? fileInfos = null;
            try
            {
                fileInfos = directoryInfo.GetFiles("*.ini");
            }
            catch (IOException ex)
            {
                Debug.WriteLine("Error reading config playlist directory: " + ex.Message);
            }
            if (fileInfos != null)
            {
                foreach (var fileInfo in fileInfos)
                {
                    var playlistTabItem = new IniDocumentTabItem(
                        PlaylistGroupControlFactory,
                        Strings.TabPlaylistConfiguration,
                        fileInfo.Name,
                        Path.Join(playlistDir, fileInfo.Name),
                        Configuration.ConfigPath.TEMPLATE_PLAYLIST_FILE_PATH
                    );
                    Tabs.Items.Add(playlistTabItem);
                }
            }

            UpdateTabsSelection();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }

        public string GetMediaPath()
        {
            var mediaPathControl = (PathControl)globalTabItem.GroupControl.GetChildByName(GlobalGroupControlFactory.MEDIA_PATH_CONTROL);
            return mediaPathControl.FullPath;
        }

        public async void CheckForUpdates()
        {
            UpdateCheckTextBlock.Inlines.Clear();
            UpdateCheckTextBlock.Inlines.Add(new Run(Strings.UpdateChecking));
            // UpdateHintVisible = true;

            List<Release> releases;
            try
            {
                releases = await updateCheckService.GetLatestReleases();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error checking for updates: " + e.Message);
                UpdateHintVisible = false;
                return;
            }

            UpdateHintVisible = releases.Count > 0;
            if (!UpdateHintVisible)
            {
                Debug.WriteLine("No updates available.");
                return;
            }

            UpdateCheckTextBlock.Inlines.Clear();
            UpdateCheckTextBlock.Inlines.Add(new Run(Strings.UpdateNewVersionAvailable));
            UpdateCheckTextBlock.Inlines.Add(" ");
            var link = new Hyperlink(new Run(string.Join(", ", releases.Select(it => it.Version))))
            {
                NavigateUri = new Uri(ReleasesLink),
            };
            link.RequestNavigate += Hyperlink_RequestNavigate;
            UpdateCheckTextBlock.Inlines.Add(link);

            var storyboard = (Storyboard)FindResource("FlashUpdateCheckBorderStoryboard");
            storyboard.Begin();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Tabs.Items)
            {
                var tabItem = (IniDocumentTabItem)item;
                SaveTabItem(tabItem);
            }
        }

        private void SaveTabItem(IniDocumentTabItem tabItem)
        {
            try
            {
                tabItem.SaveToFile();
            }
            catch (IOException)
            {
                MessageBox.Show(string.Format(Strings.ErrorWritingFile, tabItem.FilePath), AppDomain.CurrentDomain.FriendlyName);
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
            var fileName = $@"{eventData.FileName}.ini";
            var path = $@"{Configuration.ConfigPath.CONFIG_PLAYLIST_DIRECTORY_PATH}\{fileName}";
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
                Strings.TabPlaylistConfiguration,
                fileName,
                path,
                Configuration.ConfigPath.TEMPLATE_PLAYLIST_FILE_PATH
            );
            Tabs.Items.Add(playlistTabItem);
            Tabs.SelectedItem = playlistTabItem;

            // Save the new playlist file to disk.
            SaveTabItem(playlistTabItem);
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

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            StartLink(DocumentationLink);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            StartLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void StartLink(string absoluteUri)
        {
            Process.Start(new ProcessStartInfo(absoluteUri) { UseShellExecute = true });
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
