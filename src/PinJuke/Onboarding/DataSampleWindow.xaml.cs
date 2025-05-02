using PinJuke.Controller;
using PinJuke.Dof;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PinJuke.Onboarding
{
    public partial class DataSampleWindow : Window, IChangingProperties
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private BeaconService beaconService;
        private ConfigurationService configurationService;
        private Configuration.Configuration configuration;
        private Configuration.UserConfiguration userConfiguration;

        private string beaconText = "";
        public string BeaconText {
            get => beaconText;
            set => this.SetField(ref beaconText, value);
        }

        public DataSampleWindow(
            BeaconService beaconService,
            ConfigurationService configurationService,
            Configuration.Configuration configuration,
            Configuration.UserConfiguration userConfiguration
        )
        {
            this.beaconService = beaconService;
            this.configurationService = configurationService;
            this.configuration = configuration;
            this.userConfiguration = userConfiguration;

            DataContext = this;
            InitializeComponent();

            GetBeaconAsync();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged.Raise(this, propertyName);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void GetBeaconAsync()
        {
            try
            {
                BeaconText = Strings.BeaconQueryingData;
                var beacon = await Task.Run(() => GetBeacon());
                BeaconText = $"{Strings.BeaconAppName}: {beacon.AppName}\n"
                    + $"{Strings.BeaconAppVersion}: {beacon.AppVersion}\n"
                    + $"{Strings.BeaconAppFileVersion}: {beacon.AppFileVersion}\n"
                    + $"{Strings.BeaconLocale}: {beacon.Locale}\n"
                    + $"{Strings.BeaconTimezone}: {beacon.Timezone}\n"
                    + $"{Strings.BeaconDmdAvailable}: {(beacon.DmdAvailable ? Strings.Yes : Strings.No)}\n"
                    + $"{Strings.BeaconDofEnabled}: {(beacon.DofEnabled ? Strings.Yes : Strings.No)}\n"
                    + $"{Strings.BeaconControllerNames}: {string.Join(", ", beacon.ControllerNames)}\n";
            }
            catch (Exception ex)
            {
                BeaconText = string.Format(Strings.BeaconErrorQuerying, ex.Message);
            }
        }

        private Beacon GetBeacon()
        {
            var mainModel = new MainModel(configuration, userConfiguration);
            using DofMediator? dofMediator = configuration.Dof.Enabled ? new DofMediator(mainModel, configuration.Dof) : null;
            dofMediator?.Startup();
            var beaconController = new BeaconController(mainModel, beaconService, configurationService, dofMediator);
            return beaconController.GetBeacon();
        }
    }
}
