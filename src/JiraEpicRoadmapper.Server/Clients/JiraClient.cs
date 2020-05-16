using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace JiraEpicRoadmapper.Server.Clients
{
    public class JiraClient : IJiraClient
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly SemaphoreSlim _requestThrottler;
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public JiraClient(IOptions<Config> config, IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _requestThrottler = new SemaphoreSlim(config.Value.JiraQueryThroughput);
        }

        public async Task<IReadOnlyList<JsonElement>> QueryJql(string jqlQuery)
        {
            jqlQuery = Uri.EscapeDataString(jqlQuery);
            var elements = new List<JsonElement>();
            var start = 0;
            var client = GetClient();

            while (true)
            {
                var doc = await GetDocument(client, $"/rest/api/2/search?jql={jqlQuery}&startAt={start}&maxResults=50");
                var issues = doc.RootElement.GetProperty("issues");

                elements.AddRange(issues.EnumerateArray());
                start = doc.RootElement.GetProperty("startAt").GetInt32();
                var total = doc.RootElement.GetProperty("total").GetInt32();
                var maxResults = doc.RootElement.GetProperty("maxResults").GetInt32();
                start += maxResults;
                if (start >= total)
                    break;
            }
            return elements;
        }

        public async Task<IReadOnlyDictionary<string, string[]>> QueryFieldNameToKeysMap()
        {
            var doc = await GetDocument(GetClient(), "/rest/api/2/field");

            return doc.RootElement.EnumerateArray().Select(e =>
                (
                    name: e.GetProperty("name").GetString(), key:
                    e.GetProperty("key").GetString()
                ))
                .GroupBy(x => x.name)
                .ToDictionary(g => g.Key, g => g.Select(x => x.key).ToArray());
        }

        public async Task UpdateIssue(string issueKey, IssueContent issueContent)
        {
            try
            {
                await _requestThrottler.WaitAsync();
                var stringContent = new StringContent(JsonSerializer.Serialize(issueContent, JsonSerializerOptions), Encoding.UTF8, "application/json");
                using var response = await GetClient().PutAsync($"rest/api/2/issue/{issueKey}", stringContent);
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                _requestThrottler.Release();
            }
        }

        private HttpClient GetClient() => _clientFactory.CreateClient(nameof(IJiraClient));

        private async Task<JsonDocument> GetDocument(HttpClient client, string query)
        {
            try
            {
                await _requestThrottler.WaitAsync();
                using var resp = await client.GetAsync(query);
                resp.EnsureSuccessStatusCode();
                await using var stream = await resp.Content.ReadAsStreamAsync();
                return await JsonDocument.ParseAsync(stream);
            }
            finally
            {
                _requestThrottler.Release();
            }
        }
    }
}