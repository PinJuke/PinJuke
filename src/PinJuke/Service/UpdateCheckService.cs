using PinJuke.Service.Github;
using SoftCircuits.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PinJuke.Service
{
    public enum ReleaseType
    {
        Alpha,
        Beta,
        ReleaseCandidate,
        Release,
    }

    public record Release(string Version, int Major, int Minor, int Patch, ReleaseType Type, int Build);

    public class ReleaseComparer : IComparer<Release>
    {
        public int Compare(Release? x, Release? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            int result = x.Major.CompareTo(y.Major);
            if (result != 0) return result;

            result = x.Minor.CompareTo(y.Minor);
            if (result != 0) return result;

            result = x.Patch.CompareTo(y.Patch);
            if (result != 0) return result;

            result = x.Type.CompareTo(y.Type);
            if (result != 0) return result;

            return x.Build.CompareTo(y.Build);
        }
    }

    public class LatestReleaseFinder
    {
        private readonly Regex versionRegex = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<type>[a-zA-Z]+)(?:\.(?<build>\d+))?)?$");

        private readonly OrderedDictionary<string, ReleaseType> releaseTypes = new()
        {
            { "alpha", ReleaseType.Alpha },
            { "beta", ReleaseType.Beta },
            { "rc", ReleaseType.ReleaseCandidate },
            { "", ReleaseType.Release },
        };

        private readonly ReleaseComparer releaseComparer = new();

        public Release? ParseVersion(string version)
        {
            var match = versionRegex.Match(version);
            if (!match.Success)
            {
                return null;
            }
            var major = int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture);
            var minor = int.Parse(match.Groups["minor"].Value, CultureInfo.InvariantCulture);
            var patch = int.Parse(match.Groups["patch"].Value, CultureInfo.InvariantCulture);
            var typeString = match.Groups["type"].Value.ToLowerInvariant();
            if (!releaseTypes.ContainsKey(typeString))
            {
                return null;
            }
            var type = releaseTypes[typeString];
            var build = match.Groups["build"].Success ? int.Parse(match.Groups["build"].Value, CultureInfo.InvariantCulture) : 0;
            var release = new Release(version, major, minor, patch, type, build);
            return release;
        }

        public List<Release> GetLatestReleases(List<GithubRelease> githubReleases)
        {
            var releasesByType = new Dictionary<ReleaseType, Release>();
            foreach (var githubRelease in githubReleases)
            {
                if (githubRelease.Prerelease || githubRelease.TagName == null)
                {
                    continue;
                }
                var release = ParseVersion(githubRelease.TagName);
                if (release == null)
                {
                    continue;
                }
                if (!releasesByType.ContainsKey(release.Type) || releaseComparer.Compare(release, releasesByType[release.Type]) > 0)
                {
                    releasesByType[release.Type] = release;
                }
            }
            var latestReleases = new List<Release>();
            foreach (var releaseType in releaseTypes.Values.Reverse())
            {
                if (releasesByType.TryGetValue(releaseType, out var release))
                {
                    if (latestReleases.Count == 0 || releaseComparer.Compare(release, latestReleases.Last()) > 0)
                    {
                        latestReleases.Add(release);
                    }
                }
            }
            return latestReleases;
        }

        public List<Release> GetLatestReleasesAfter(Release release, List<Release> latestReleases)
        {
            return latestReleases.FindAll(r => releaseComparer.Compare(r, release) > 0);
        }
    }

    public class UpdateCheckService
    {
        private readonly GithubReleaseService githubReleaseService = new();
        private readonly LatestReleaseFinder latestReleaseFinder = new();

        public async Task<List<Release>> GetLatestReleases()
        {
            var githubReleases = await githubReleaseService.GetReleases("PinJuke", "PinJuke");

            var latestReleases = latestReleaseFinder.GetLatestReleases(githubReleases);
            var release = GetAssemblyRelease();
            if (release != null)
            {
                latestReleases = latestReleaseFinder.GetLatestReleasesAfter(release, latestReleases);
            }
            return latestReleases;
        }

        private Release? GetAssemblyRelease()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var productVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
            var release = latestReleaseFinder.ParseVersion(productVersion);
            return release;
        }
    }
}
