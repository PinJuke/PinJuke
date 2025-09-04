using PinJuke.Controller;
using PinJuke.Dof;
using PinJuke.Ini;
using PinJuke.Model;
using PinJuke.Service;
using PinJuke.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PinJuke.Onboarding
{
    public record TemplateInfo(DataTemplate Template, Func<bool>? ValidFunc = null);

    public record FinishEventData(bool CreatePlaylist);

    public record InputEntry(string ImagePath, string Button, string FunctionNormal, string FunctionBrowser);

    public partial class OnboardingWindow : Window, IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<FinishEventData>? FinishEvent;

        private Page page = new();
        private int pageIndex = 0;
        private TemplateInfo[] templates;

        public ImageSource LogoImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\pinjuke.svg");
        }

        public ImageSource FinishImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\pinball.svg");
        }

        public ImageSource WarningImageSource
        {
            get => SvgImageLoader.Instance.GetFromResource(@"icons\warning-outline.svg");
        }

        public string ProjectLink
        {
            get => "https://github.com/PinJuke/PinJuke";
        }

        private string? licenseText = null;
        public string LicenseText
        {
            get
            {
                if (licenseText == null)
                {
                    var uri = new Uri(@"resources\LICENSE", UriKind.Relative);
                    using var stream = Application.GetResourceStream(uri).Stream;
                    licenseText = new StreamReader(stream).ReadToEnd();
                }
                return licenseText;
            }
        }

        public ObservableCollection<InputEntry> InputEntries { get; } = new()
        {
            new InputEntry("pack://application:,,,/icons/buttons/flipper-button.jpg", Strings.ButtonFlipperLeftRight, Strings.ButtonFlipperLeftRightRegularFunction, Strings.ButtonFlipperLeftRightBrowserFunction),
            new InputEntry("pack://application:,,,/icons/buttons/magna-save-button.jpg", Strings.ButtonMagnaSaveLeftRight, Strings.ButtonMagnaSaveLeftRightRegularFunction, Strings.ButtonMagnaSaveLeftRightBrowserFunction),
            new InputEntry("pack://application:,,,/icons/buttons/start-button.jpg", Strings.ButtonStart, Strings.ButtonStartRegularFunction, Strings.ButtonStartBrowserFunction),
            new InputEntry("pack://application:,,,/icons/buttons/exit-button.jpg", Strings.ButtonExit, Strings.ButtonExitRegularFunction, Strings.ButtonExitBrowserFunction),
            new InputEntry("pack://application:,,,/icons/buttons/launch-ball-button.jpg", Strings.ButtonLaunchBall, Strings.ButtonLaunchBallRegularFunction, Strings.ButtonLaunchBallBrowserFunction),
        };

        public Model Model { get; } = new();

        private BeaconService beaconService;
        private ConfigurationService configurationService;
        private IniDocument iniDocument;
        private Configuration.Configuration configuration;
        private Configuration.UserConfiguration userConfiguration;

        public OnboardingWindow(BeaconService beaconService, ConfigurationService configurationService)
        {
            this.beaconService = beaconService;
            this.configurationService = configurationService;

            DataContext = this;
            InitializeComponent();
            templates =
            [
                new TemplateInfo((DataTemplate)Resources["WelcomeTemplate"]),
                new TemplateInfo((DataTemplate)Resources["LicenseTemplate"], IsLicenseValid),
                new TemplateInfo((DataTemplate)Resources["DisplayTemplate"]),
                new TemplateInfo((DataTemplate)Resources["DofTemplate"]),
                new TemplateInfo((DataTemplate)Resources["DataCollectionTemplate"], IsDataCollectionValid),
                new TemplateInfo((DataTemplate)Resources["KeyboardMappingTemplate"]),
                new TemplateInfo((DataTemplate)Resources["FinishTemplate"]),
            ];
            AddPage(0);
            page.AnimateInitial();

            iniDocument = IniReader.TryRead(Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH)
                ?? IniReader.TryRead(Configuration.ConfigPath.TEMPLATE_GLOBAL_FILE_PATH)
                ?? new IniDocument();
            configuration = new Configuration.ConfigurationLoader().FromIniDocument(iniDocument);
            try
            {
                userConfiguration = configurationService.LoadUserConfiguration();
            }
            catch (IniIoException ex)
            {
                Debug.WriteLine("Error reading user configuration ini file: " + ex.Message);
                userConfiguration = new(new IniDocument(), new Configuration.Parser());
            }

            PopulateModel();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }

        private void PopulateModel()
        {
            Model.LicenseAccepted = userConfiguration.SetUp;
            PopulateDisplay(Model.PlayFieldDisplay, configuration.PlayField.Window);
            PopulateDisplay(Model.BackGlassDisplay, configuration.BackGlass.Window);
            PopulateDisplay(Model.DmdDisplay, configuration.Dmd.Window);
            Model.DmdEnabled = configuration.Dmd.Enabled;
            Model.DofEnabled = configuration.Dof.Enabled;
            Model.DofPath = configuration.Dof.GlobalConfigFilePath;
            Model.Consent = userConfiguration.SetUp ? userConfiguration.BeaconEnabled : null;
            Model.UpdateCheckEnabled = userConfiguration.UpdateCheckEnabled ?? true;
            Model.CreatePlaylist = !userConfiguration.SetUp;
            CheckDofPathInvalid();
        }

        private void PopulateDisplay(Display display, Configuration.Window window)
        {
            display.Left = window.Left;
            display.Top = window.Top;
            display.Width = window.Width;
            display.Height = window.Height;
        }

        private void CheckDofPathInvalid()
        {
            Model.DofPathInvalid = Model.DofPath.IsNullOrEmpty() || !File.Exists(Model.DofPath);
        }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Model.DofPath):
                    CheckDofPathInvalid();
                    break;
            }
        }

        private void AddPage(int index)
        {
            pageIndex = index;
            BackButton.IsEnabled = index > 0;
            NextButton.Content = index + 1 < templates.Length ? Strings.Next : Strings.Finish;
            CheckValid();

            page = new();
            page.RemovalRequestedEvent += Page_RemovalRequestedEvent;
            page.ContentTemplate = templates[index].Template;
            PageContainer.Children.Add(page);
        }

        private void CheckValid()
        {
            NextButton.IsEnabled = templates[pageIndex].ValidFunc?.Invoke() ?? true;
        }

        private void Page_RemovalRequestedEvent(object? sender, EventArgs e)
        {
            Debug.WriteLine("Removing page.");
            PageContainer.Children.Remove((Page)sender!);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            page.AnimateBackOut();
            AddPage(pageIndex - 1);
            page.AnimateBackIn();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (pageIndex + 1 >= templates.Length)
            {
                Finish();
                return;
            }
            page.AnimateNextOut();
            AddPage(pageIndex + 1);
            page.AnimateNextIn();
        }

        private void Finish()
        {
            var configuration = GetConfiguration();
            var userConfiguration = GetUserConfiguration();
            configurationService.SaveGlobalConfiguration(configuration, iniDocument);
            configurationService.SaveUserConfiguration(userConfiguration);
            FinishEvent?.Invoke(this, new FinishEventData(Model.CreatePlaylist));
        }

        private bool IsLicenseValid()
        {
            return Model.LicenseAccepted;
        }

        private bool IsDataCollectionValid()
        {
            return Model.Consent != null;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void License_Checked(object sender, RoutedEventArgs e)
        {
            CheckValid();
        }

        private void DataCollection_Checked(object sender, RoutedEventArgs e)
        {
            CheckValid();
        }

        private void GetFromPinUpButton_Click(object sender, RoutedEventArgs e)
        {
            bool success;
            try
            {
                success = SetDisplayFromPinUp(Model.PlayFieldDisplay, Configurator.PinUpPlayerIniReader.PLAY_FIELD_SECTION_NAME)
                    && SetDisplayFromPinUp(Model.BackGlassDisplay, Configurator.PinUpPlayerIniReader.BACK_GLASS_SECTION_NAME)
                    && SetDisplayFromPinUp(Model.DmdDisplay, Configurator.PinUpPlayerIniReader.DMD_SECTION_NAME);
            }
            catch (IniIoException ex)
            {
                MessageBox.Show(string.Format(Strings.ErrorReadingFile, ex.FilePath), AppDomain.CurrentDomain.FriendlyName);
                return;
            }
            if (!success)
            {
                MessageBox.Show(string.Format(Strings.PathNotFound, Configurator.PinUpPlayerIniReader.BALLER_PIN_UP_PLAYER_INI), AppDomain.CurrentDomain.FriendlyName);
            }
        }

        private bool SetDisplayFromPinUp(Display display, string sectionName)
        {
            var pinUpReader = Configurator.PinUpPlayerIniReader.Create();
            var position = pinUpReader.FindPosition(sectionName);
            if (position == null)
            {
                return false;
            }
            display.Left = position.Value.Item1;
            display.Top = position.Value.Item2;
            display.Width = position.Value.Item3;
            display.Height = position.Value.Item4;
            return true;
        }

        private void ChooseDof_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".xml"; // Default file extension
            dialog.Filter = $"{Strings.XmlFile}|*.xml"; // Filter files by extension
            bool? result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            Model.DofPath = dialog.FileName;
        }

        private void DataSample_Click(object sender, RoutedEventArgs e)
        {
            var configuration = GetConfiguration();
            var userConfiguration = GetUserConfiguration();
            var dataSampleWindow = new DataSampleWindow(beaconService, configurationService, configuration, userConfiguration);
            dataSampleWindow.Owner = this;
            dataSampleWindow.ShowDialog();
        }

        private Configuration.Configuration GetConfiguration()
        {
            var configuration = this.configuration;
            var playField = configuration.PlayField with { Enabled = true, Window = GetWindow(Model.PlayFieldDisplay, true) };
            var backGlass = configuration.BackGlass with { Enabled = true, Window = GetWindow(Model.BackGlassDisplay) };
            var dmd = configuration.Dmd with { Enabled = Model.DmdEnabled, Window = GetWindow(Model.DmdDisplay) };
            var dof = configuration.Dof with { Enabled = Model.DofEnabled, GlobalConfigFilePath = Model.DofPath };
            return configuration with { PlayField = playField, BackGlass = backGlass, Dmd = dmd, Dof = dof };
        }

        private Configuration.Window GetWindow(Display display, bool isVertical = false)
        {
            var is4k = display.Width >= 3840 || display.Height >= 3840;
            return new Configuration.Window(
                display.Left,
                display.Top,
                display.Width,
                display.Height,
                is4k ? 2f : 1f,
                isVertical ? -90 : 0
            );
        }

        private Configuration.UserConfiguration GetUserConfiguration()
        {
            var userConfiguration = this.userConfiguration;
            userConfiguration.SetUp = true;
            userConfiguration.UpdateCheckEnabled = Model.UpdateCheckEnabled;
            userConfiguration.BeaconEnabled = Model.Consent;
            return userConfiguration;
        }
    }
}
