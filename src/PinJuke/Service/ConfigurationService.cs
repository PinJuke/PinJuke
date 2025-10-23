using PinJuke.Configuration;
using PinJuke.Ini;
using PinJuke.Model;
using PinJuke.Utility;
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

        public void SaveGlobalConfiguration(Configuration.Configuration configuration, IniDocument iniDocument)
        {
            var loader = new Configuration.ConfigurationLoader();
            loader.ToGlobalIniDocument(configuration, iniDocument);

            var globalConfigFile = Configuration.ConfigPath.CONFIG_GLOBAL_FILE_PATH;
            CreateDirectoryForFile(globalConfigFile);
            using var textWriter = new StreamWriter(globalConfigFile);
            iniDocument.WriteTo(textWriter);
        }

        private string GetUserConfigurationFilePath()
        {
            var userConfigDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PinJuke";
            var userConfigFile = Path.Join(userConfigDir, Configuration.ConfigPath.USER_FILE_NAME);
            return userConfigFile;
        }

        public Configuration.UserConfiguration LoadUserConfiguration()
        {
            var userConfigFile = GetUserConfigurationFilePath();

            var loader = new Configuration.UserConfigurationLoader();
            return loader.FromIniFilePath(userConfigFile);
        }

        public void SaveUserConfiguration(Configuration.UserConfiguration userConfiguration)
        {
            var userConfigFile = GetUserConfigurationFilePath();
            CreateDirectoryForFile(userConfigFile);

            using var textWriter = new StreamWriter(userConfigFile);
            userConfiguration.IniDocument.WriteTo(textWriter);
        }

        public Configuration.DistributionInfo LoadDistributionInfo()
        {
            var loader = new Configuration.DistributionInfoLoader();
            return loader.FromIniFile();
        }

        private void CreateDirectoryForFile(string filePath)
        {
            FileUtil.CreateDirectoryForFile(filePath);
        }
    }
}
