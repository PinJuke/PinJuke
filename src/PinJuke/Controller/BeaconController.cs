using PinJuke.Model;
using PinJuke.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PinJuke.Controller
{
    public class BeaconController
    {
        private MainModel mainModel;
        private BeaconService beaconService;
        private ConfigurationService configurationService;

        public BeaconController(MainModel mainModel, BeaconService beaconService, ConfigurationService configurationService)
        {
            this.mainModel = mainModel;
            this.beaconService = beaconService;
            this.configurationService = configurationService;
        }

        public Beacon GetBeacon()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var beacon = new Beacon(
                assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "",
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "",
                assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "",
                CultureInfo.CurrentCulture.Name,
                TimeZoneInfo.Local.Id,
                mainModel.Configuration.Dmd.Enabled,
                mainModel.Configuration.Dof.Enabled
            );
            return beacon;
        }

        public async Task Startup()
        {
            var userConfiguration = mainModel.UserConfiguration;
            if (userConfiguration.BeaconEnabled != true)
            {
                return;
            }

            var now = DateTime.Now;
            if (userConfiguration.LastBeaconSentAt != null)
            {
                if (DateTime.TryParse(userConfiguration.LastBeaconSentAt, out var lastTime))
                {
                    var span = now - lastTime;
                    if (span.TotalHours < 6)
                    {
                        return;
                    }
                }
            }

            InstallIdPair? installIdPair = null;
            if (userConfiguration.PrivateId != null && userConfiguration.PublicId != null)
            {
                try
                {
                    installIdPair = new InstallIdPair(userConfiguration.PrivateId, userConfiguration.PublicId);
                }
                catch (ArgumentException exception)
                {
                    Debug.WriteLine("Error getting install id pair from user configuration: " + exception.Message);
                }
            }
            if (installIdPair == null)
            {
                installIdPair = InstallIdPair.Generate();
                userConfiguration.PrivateId = installIdPair.PrivateId;
                userConfiguration.PublicId = installIdPair.PublicId;
            }

            if (await SendBeacon(installIdPair))
            {
                userConfiguration.LastBeaconSentAt = now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            }

            this.configurationService.SaveUserConfiguration(userConfiguration);
        }

        private async Task<bool> SendBeacon(InstallIdPair installIdPair)
        {
            Beacon beacon;
            try
            {
                beacon = GetBeacon();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Error getting beacon: " + exception.Message);
                return false;
            }
            try
            {
                await beaconService.SendBeacon(installIdPair, beacon);
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Error sending beacon: " + exception.Message);
                return false;
            }
            return true;
        }
    }
}
