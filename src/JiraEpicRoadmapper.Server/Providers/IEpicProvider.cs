using System.Collections.Generic;
using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.Server.Providers
{
    public interface IEpicProvider
    {
        Task<IEnumerable<Epic>> GetEpics();
        Task<EpicStats> GetEpicStats(string epicKey);
        Task<Epic> UpdateEpic(string epicKey, EpicMeta meta);
    }
}
