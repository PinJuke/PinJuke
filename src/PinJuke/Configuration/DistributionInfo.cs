using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Configuration
{
    public record DistributionInfo(string DownloadLink, string UpdateCheckGithubOwner, string UpdateCheckGithubRepo);
}
