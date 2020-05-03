using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

        public EpicProvider(IJiraClient client, IEpicMapper mapper, IOptions<Config> config)
        {
            _client = client;
            _mapper = mapper;
            _config = config;
        }

        public async Task<IEnumerable<Epic>> GetEpics()
        {
            var jql = GetJql();
            var epics = await _client.QueryJql(jql);

            return epics.Select(MapEpic);
        }

        private string GetJql()
        {
            var epicFilter = _config.Value.EpicQueryFilter;

            var jql = "issuetype=Epic";
            if (!string.IsNullOrWhiteSpace(epicFilter))
                jql += $" AND ({epicFilter})";
            
            return jql;
        }

        private Epic MapEpic(JsonElement e)
        {
            return _mapper.MapEpic(e);
        }
    }
}