using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Repositories
{
    public class EpicsRepository : IEpicsRepository
    {
        private readonly HttpClient _client;

        public EpicsRepository(HttpClient client)
        {
            _client = client;
        }

        public Task<Epic[]> FetchEpics()
        {
            return _client.GetFromJsonAsync<Epic[]>("/api/epics");
        }

        public Task<EpicStats> FetchEpicStats(string epicKey)
        {
            return _client.GetFromJsonAsync<EpicStats>($"/api/epics/{epicKey}/stats");
        }
    }
}