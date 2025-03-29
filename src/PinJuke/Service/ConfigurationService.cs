using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Service
{
    public class ConfigurationService
    {
        public ConfigurationService()
        {
        }

        public Configuration.Configuration LoadConfiguration(string? playlistConfigFilePath)
        {
            List<string> iniFilePaths = new();
            iniFilePaths.Add(Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH);
            if (playlistConfigFilePath != null)
            {
                iniFilePaths.Add(playlistConfigFilePath);
            }

            var loader = new Configuration.ConfigurationLoader();
            return loader.FromIniFilePaths(iniFilePaths, playlistConfigFilePath);
        }

        public Configuration.UserConfiguration LoadUserConfiguration()
        {
            var userConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PinJuke";
            var userConfigFile = Path.Combine(userConfigDir, Configuration.ConfigPath.USER_FILE_NAME);

            var loader = new Configuration.UserConfigurationLoader();
            return loader.FromIniFilePath(userConfigFile);
        }

        public void SaveUserConfiguration(Configuration.UserConfiguration userConfiguration)
        {
            var userConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PinJuke";
            var userConfigFile = Path.Combine(userConfigDir, Configuration.ConfigPath.USER_FILE_NAME);

            Directory.CreateDirectory(userConfigDir);
            using var textWriter = new StreamWriter(userConfigFile);
            userConfiguration.IniDocument.WriteTo(textWriter);
        }
    }
}
