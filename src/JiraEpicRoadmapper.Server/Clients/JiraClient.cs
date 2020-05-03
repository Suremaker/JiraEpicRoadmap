using System;
using System.Collections.Generic;
using System.Net.Http;
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
            var client = _clientFactory.CreateClient(nameof(IJiraClient));

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