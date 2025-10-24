using PinJuke.Ini;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PinJuke.Configuration
{
    public class DistributionInfoLoader
    {
        protected readonly Parser parser = new();

        public DistributionInfo FromIniFile()
        {
            var iniFilePath = Path.Join(AppContext.BaseDirectory, ConfigPath.DISTRIBUTION_INFO_FILE_NAME);
            var iniDocument = IniReader.TryRead(iniFilePath) ?? new IniDocument();
            return FromIniDocument(iniDocument);
        }

        public DistributionInfo FromIniDocument(IniDocument iniDocument)
        {
            var appSection = iniDocument["App"];
            return new DistributionInfo(
                parser.ParseString(appSection["DownloadLink"]) ?? "https://github.com/PinJuke/PinJuke/releases",
                parser.ParseString(appSection["UpdateCheckGithubOwner"]) ?? "PinJuke",
                parser.ParseString(appSection["UpdateCheckGithubRepo"]) ?? "PinJuke",
                parser.ParseString(appSection["PackageVersion"]) ?? GetAssemblyVersion()
            );
        }

        private string GetAssemblyVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var productVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
            return productVersion;
        }
    }
}
