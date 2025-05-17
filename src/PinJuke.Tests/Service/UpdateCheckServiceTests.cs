using System;
using System.Collections.Generic;
using PinJuke.Service;
using PinJuke.Service.Github;
using Xunit;

namespace PinJuke.Tests.Service
{
    public class LatestReleaseFinderTests
    {
        [Theory]
        [InlineData("1.2.3", 1, 2, 3, ReleaseType.Release, 0)]
        [InlineData("1.2.3-alpha.5", 1, 2, 3, ReleaseType.Alpha, 5)]
        [InlineData("2.0.0-beta", 2, 0, 0, ReleaseType.Beta, 0)]
        [InlineData("3.1.4-rc.2", 3, 1, 4, ReleaseType.ReleaseCandidate, 2)]
        public void ParseVersion_ValidVersions_ReturnsExpectedRelease(
            string version, int major, int minor, int patch, ReleaseType type, int build)
        {
            var finder = new LatestReleaseFinder();
            var release = finder.ParseVersion(version);

            Assert.NotNull(release);
            Assert.Equal(major, release.Major);
            Assert.Equal(minor, release.Minor);
            Assert.Equal(patch, release.Patch);
            Assert.Equal(type, release.Type);
            Assert.Equal(build, release.Build);
        }

        [Fact]
        public void ParseVersion_InvalidVersion_ReturnsNull()
        {
            var finder = new LatestReleaseFinder();
            Assert.Null(finder.ParseVersion("not-a-version"));
        }

        [Fact]
        public void GetLatestReleases_ReturnsLatest()
        {
            var finder = new LatestReleaseFinder();
            var githubReleases = new List<GithubRelease>
            {
                new GithubRelease { TagName = "1.0.0-beta.1", Prerelease = false },
                new GithubRelease { TagName = "1.0.1-beta.2", Prerelease = false },
                new GithubRelease { TagName = "1.0.0", Prerelease = false },
                new GithubRelease { TagName = "1.0.2-alpha.3", Prerelease = false },
                new GithubRelease { TagName = "1.0.0-rc.1", Prerelease = false },
            };

            var latestReleases = finder.GetLatestReleases(githubReleases);
            var expected = new List<Release>
            {
                new Release("1.0.0", 1, 0, 0, ReleaseType.Release, 0),
                new Release("1.0.1-beta.2", 1, 0, 1, ReleaseType.Beta, 2),
                new Release("1.0.2-alpha.3", 1, 0, 2, ReleaseType.Alpha, 3),
            };
            Assert.Equal(expected, latestReleases);
        }
    }
}