using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;
using JiraEpicRoadmapper.Server.Clients;
using JiraEpicRoadmapper.Server.Mappers;
using Microsoft.Extensions.Options;

namespace JiraEpicRoadmapper.Server.Providers
{
    public class EpicProvider : IEpicProvider
    {
        private readonly IJiraClient _client;
        private readonly IEpicMapper _mapper;
        private readonly IOptions<Config> _config;
        private readonly SemaphoreSlim _fieldRetrieveLock = new SemaphoreSlim(1);
        private IReadOnlyDictionary<string, string[]> _fieldsNameToKeysMap;

        public EpicProvider(IJiraClient client, IEpicMapper mapper, IOptions<Config> config)
        {
            _client = client;
            _mapper = mapper;
            _config = config;
        }

        public async Task<IEnumerable<Epic>> GetEpics()
        {
            var fields = await GetFields();
            var jql = GetJql();
            var epics = await _client.QueryJql(jql);

            return epics.Select(e => MapEpic(e, fields));
        }

        public async Task<EpicStats> GetEpicStats(string epicKey)
        {
            var result = await _client.QueryJql($"\"Epic Link\"={epicKey}");
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

        public async Task<Epic> UpdateEpic(string epicKey, EpicMeta meta)
        {
            var fieldsNameToKeyMap = await GetFields();
            var content = MapMetaToIssueContent(meta, fieldsNameToKeyMap);
            await _client.UpdateIssue(epicKey, content);
            var epics = await _client.QueryJql($"key={epicKey}");
            return _mapper.MapEpic(epics.Single(), fieldsNameToKeyMap);
        }

        private async Task<IReadOnlyDictionary<string, string[]>> GetFields()
        {
            if (_fieldsNameToKeysMap != null)
                return _fieldsNameToKeysMap;

            try
            {
                await _fieldRetrieveLock.WaitAsync();
                return _fieldsNameToKeysMap ??= await _client.QueryFieldNameToKeysMap();
            }
            finally
            {
                _fieldRetrieveLock.Release();
            }
        }

        private string GetJql()
        {
            var epicFilter = _config.Value.EpicQueryFilter;

            var jql = "issuetype=Epic";
            if (!string.IsNullOrWhiteSpace(epicFilter))
                jql += $" AND ({epicFilter})";

            return jql;
        }

        private Epic MapEpic(JsonElement e, IReadOnlyDictionary<string, string[]> fieldsNameToKeyMap)
        {
            return _mapper.MapEpic(e, fieldsNameToKeyMap);
        }

        private static IssueContent MapMetaToIssueContent(EpicMeta meta, IReadOnlyDictionary<string, string[]> map)
        {
            var content = new IssueContent
            {
                Fields =
                {
                    {"duedate", meta.DueDate?.ToString("yyyy-MM-dd")}
                }
            };

            var startDateField = map.TryGetValue("Start date", out var values) ? values.FirstOrDefault() : null;
            if (startDateField != null) content.Fields[startDateField] = meta.StartDate?.ToString("yyyy-MM-dd");
            return content;
        }
    }
}