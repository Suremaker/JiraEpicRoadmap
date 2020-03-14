using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace JiraEpicRoadmapper.Server.Clients
{
    public class JiraClient : IJiraClient
    {
        private readonly SemaphoreSlim _initSemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _requestThrottler = new SemaphoreSlim(10);
        private readonly IHttpClientFactory _clientFactory;
        private readonly IOptions<Config> _config;
        private readonly JiraMapper _mapper;
        private IReadOnlyDictionary<string, string[]> _fieldMap;

        public JiraClient(IHttpClientFactory clientFactory, IOptions<Config> config, JiraMapper mapper)
        {
            _clientFactory = clientFactory;
            _config = config;
            _mapper = mapper;
        }

        public async Task<Epic[]> GetEpics()
        {
            var config = _config.Value;
            var client = _clientFactory.CreateClient(nameof(IJiraClient));
            var fields = await GetFieldMaps(client);

            var query = "issuetype=Epic";
            if (!string.IsNullOrWhiteSpace(config.EpicQueryFilter))
                query += $" AND ({config.EpicQueryFilter})";

            var response = await Query(client, query);
            return _mapper.ParseEpics(response, fields);
        }

        public async Task<EpicStats> GetEpicStats(string epicKey)
        {
            var client = _clientFactory.CreateClient(nameof(IJiraClient));
            var result = await Query(client, $"\"Epic Link\"={epicKey}");
            var statuses = result
                .Select(t => t.GetProperty("fields").GetProperty("status").GetProperty("statusCategory").GetProperty("name").GetString())
                .GroupBy(x => x);

            var stats = new EpicStats();
            foreach (var grp in statuses)
            {
                if (grp.Key.Equals("done", StringComparison.OrdinalIgnoreCase))
                    stats.Done += grp.Count();
                else if (grp.Key.Equals("in progress", StringComparison.OrdinalIgnoreCase))
                    stats.InProgress += grp.Count();
                else
                    stats.NotStarted += grp.Count();
            }

            return stats;
        }

        class FieldsRequest
        {
            public IDictionary<string, string> Fields { get; set; }
        }

        public async Task UpdateEpic(string epicKey, EpicMeta meta)
        {
            var client = _clientFactory.CreateClient(nameof(IJiraClient));
            var map = await GetFieldMaps(client);
            var fields = new FieldsRequest
            {
                Fields = new Dictionary<string, string>
                {
                    {"duedate", meta.DueDate?.ToString("yyyy-MM-dd")}
                }
            };

            var startDateField = map.TryGetValue("Start date", out var values) ? values.FirstOrDefault() : null;
            if (startDateField != null) fields.Fields.Add(startDateField, meta.StartDate?.ToString("yyyy-MM-dd"));

            var content = JsonSerializer.Serialize(fields, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var response = await client.PutAsync($"rest/api/2/issue/{epicKey}", new StringContent(content, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        private async Task<IReadOnlyDictionary<string, string[]>> GetFieldMaps(HttpClient client)
        {
            if (_fieldMap != null)
                return _fieldMap;

            try
            {
                await _initSemaphore.WaitAsync();

                if (_fieldMap != null)
                    return _fieldMap;
                return _fieldMap = await QueryFieldMap(client);
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        private async Task<IReadOnlyDictionary<string, string[]>> QueryFieldMap(HttpClient client)
        {
            var doc = await GetDocument(client, "/rest/api/2/field");

            return doc.RootElement.EnumerateArray().Select(e =>
                (
                    name: e.GetProperty("name").GetString(), key:
                    e.GetProperty("key").GetString()
                ))
                .GroupBy(x => x.name)
                .ToDictionary(g => g.Key, g => g.Select(x => x.key).ToArray());
        }

        private async Task<IReadOnlyList<JsonElement>> Query(HttpClient client, string query)
        {
            query = Uri.EscapeDataString(query);
            var elements = new List<JsonElement>();
            var start = 0;
            while (true)
            {
                var doc = await GetDocument(client, $"/rest/api/2/search?jql={query}&startAt={start}&maxResults=50");
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
                return await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
            }
            finally
            {
                _requestThrottler.Release();
            }
        }
    }
}