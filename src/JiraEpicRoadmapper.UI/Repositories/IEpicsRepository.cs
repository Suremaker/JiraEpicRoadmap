using System.Threading.Tasks;
using JiraEpicRoadmapper.Contracts;

namespace JiraEpicRoadmapper.UI.Repositories
{
    public interface IEpicsRepository
    {
        Task<Epic[]> FetchEpics();
        Task<EpicStats> FetchEpicStats(string epicKey);
        Task<Epic> UpdateEpicMetadata(string epicKey, EpicMeta meta);
    }
}
