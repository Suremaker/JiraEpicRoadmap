﻿using System.Collections.Generic;
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
    }
}