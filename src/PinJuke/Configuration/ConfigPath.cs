using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Configuration
{
    public class ConfigPath
    {
        public const string TEMPLATE_GLOBAL_FILE_PATH = @"Templates\PinJuke.global.sample.ini";
        public const string TEMPLATE_PLAYLIST_FILE_PATH = @"Templates\PinJuke.playlist.sample.ini";

        public const string CONFIG_GLOBAL_FILE_PATH = @"Configs\PinJuke.global.ini";
        public const string CONFIG_PLAYLIST_DIRECTORY_PATH = @"Configs\Playlists";

        public const string USER_FILE_NAME = @"PinJuke.user.ini";

        public const string DISTRIBUTION_INFO_FILE_NAME = @"DistributionInfo.ini";
    }
}
