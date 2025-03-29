using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PinJuke.Ini;

namespace PinJuke.Configuration
{
    public class UserConfigurationLoader
    {
        protected readonly Parser parser = new();

        protected readonly Regex playlistRegex = new Regex(@"^Playlist(\d+)$", RegexOptions.IgnoreCase);

        public UserConfiguration FromIniFilePath(string iniFilePath)
        {
            var iniReader = new IniReader();
            var iniDocument = iniReader.Read(iniFilePath);
            return FromIniDocument(iniDocument);
        }

        public UserConfiguration FromIniDocument(IniDocument iniDocument)
        {
            var userSection = iniDocument["User"];
            var userConfiguration = new UserConfiguration(
                iniDocument,
                parser,
                parser.ParseString(userSection["PrivateId"]),
                parser.ParseString(userSection["PublicId"]),
                parser.ParseBool(userSection["UpdateCheckEnabled"]),
                parser.ParseBool(userSection["BeaconEnabled"]),
                parser.ParseString(userSection["LastBeaconSentAt"])
            );
            foreach (var (name, iniSection) in iniDocument)
            {
                var match = playlistRegex.Match(name);
                if (match.Success)
                {
                    var index = int.Parse(match.Groups[1].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture);
                    var playlistConfigFilePath = parser.ParseString(iniSection["PlaylistConfigFilePath"]) ?? "";
                    var trackFilePath = parser.ParseString(iniSection["TrackFilePath"]) ?? "";
                    var userPlaylist = new UserPlaylist(iniSection, parser, index, playlistConfigFilePath, trackFilePath);
                    userConfiguration.AddPlaylist(userPlaylist);
                }
            }
            return userConfiguration;
        }
    }
}
