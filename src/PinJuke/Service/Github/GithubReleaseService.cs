using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace PinJuke.Service.Github
{
    public class GithubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset PublishedAt { get; set; }
    }

    public class GithubReleaseService
    {
        private readonly HttpClient httpClient = new HttpClient();

        public async Task<List<GithubRelease>> GetReleases(string owner, string repo)
        {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases?per_page=100&page=1";
            httpClient.DefaultRequestHeaders.UserAgent.Add(CreateUserAgentHeader());
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            var httpResponseMessage = await httpClient.GetAsync(url);
            httpResponseMessage.EnsureSuccessStatusCode();
            EnsureJsonContentType(httpResponseMessage);
            using var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var releases = await JsonSerializer.DeserializeAsync<List<GithubRelease>>(stream);
            if (releases == null)
            {
                throw new JsonException("Failed to deserialize GitHub releases.");
            }
            return releases;
        }

        private void EnsureJsonContentType(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.Content.Headers.ContentType?.MediaType != "application/json")
            {
                throw new HttpRequestException(@"""application/json"" response expected.");
            }
        }

        private ProductInfoHeaderValue CreateUserAgentHeader()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var productName = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Unknown";
            var productVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
            return new ProductInfoHeaderValue(productName, productVersion);
        }
    }
}
